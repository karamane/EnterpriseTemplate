using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Xml;
using Enterprise.Core.Application.Interfaces.Logging;
using Enterprise.Core.Application.Models.Logging;
using Microsoft.Extensions.Logging;

namespace Enterprise.Proxy.Core.Wcf;

/// <summary>
/// WCF Client Message Inspector
/// Request ve Response'ları otomatik loglar
/// </summary>
public class WcfLoggingInspector : IClientMessageInspector
{
    private readonly ILogger _logger;
    private readonly ICorrelationContext _correlationContext;
    private readonly ILogService _logService;
    private readonly string _serviceName;

    public WcfLoggingInspector(
        ILogger logger,
        ICorrelationContext correlationContext,
        ILogService logService,
        string serviceName)
    {
        _logger = logger;
        _correlationContext = correlationContext;
        _logService = logService;
        _serviceName = serviceName;
    }

    /// <summary>
    /// Request gönderilmeden önce çağrılır
    /// </summary>
    public object? BeforeSendRequest(ref Message request, IClientChannel channel)
    {
        var correlationState = new WcfCorrelationState
        {
            StartTime = DateTime.UtcNow,
            Action = request.Headers.Action,
            CorrelationId = _correlationContext.CorrelationId
        };

        // Request body'yi oku
        string requestBody = GetMessageBody(ref request);
        correlationState.RequestBody = requestBody;

        // Correlation ID'yi SOAP header'a ekle
        AddCorrelationIdHeader(ref request);

        // Request'i logla
        _logger.LogInformation(
            "[{CorrelationId}] WCF Request: {ServiceName}.{Action}",
            _correlationContext.CorrelationId,
            _serviceName,
            GetActionName(request.Headers.Action));

        // Async request log
        _ = LogRequestAsync(correlationState, requestBody);

        return correlationState;
    }

    /// <summary>
    /// Response alındıktan sonra çağrılır
    /// </summary>
    public void AfterReceiveReply(ref Message reply, object correlationState)
    {
        var state = correlationState as WcfCorrelationState;
        if (state == null) return;

        var duration = (DateTime.UtcNow - state.StartTime).TotalMilliseconds;

        // Response body'yi oku
        string responseBody = GetMessageBody(ref reply);

        // Hata kontrolü
        bool isFault = reply.IsFault;

        var logLevel = isFault ? LogLevel.Warning : LogLevel.Information;

        _logger.Log(logLevel,
            "[{CorrelationId}] WCF Response: {ServiceName}.{Action} | Duration: {Duration}ms | Fault: {IsFault}",
            state.CorrelationId,
            _serviceName,
            GetActionName(state.Action),
            duration,
            isFault);

        // Async response log
        _ = LogResponseAsync(state, responseBody, duration, isFault);
    }

    private void AddCorrelationIdHeader(ref Message request)
    {
        // Correlation ID'yi SOAP header olarak ekle
        var header = MessageHeader.CreateHeader(
            "X-Correlation-ID",
            "http://enterprise.com/logging",
            _correlationContext.CorrelationId);

        request.Headers.Add(header);
    }

    private static string GetMessageBody(ref Message message)
    {
        if (message == null) return string.Empty;

        try
        {
            // Message'ı buffer'a kopyala (okuduktan sonra tekrar kullanabilmek için)
            var buffer = message.CreateBufferedCopy(int.MaxValue);
            message = buffer.CreateMessage();

            using var copyMessage = buffer.CreateMessage();
            using var stringWriter = new StringWriter();
            using var xmlWriter = XmlWriter.Create(stringWriter, new XmlWriterSettings { Indent = true });

            copyMessage.WriteMessage(xmlWriter);
            xmlWriter.Flush();

            return stringWriter.ToString();
        }
        catch
        {
            return "[Could not read message body]";
        }
    }

    private static string GetActionName(string? action)
    {
        if (string.IsNullOrEmpty(action)) return "Unknown";

        // Action URL'den metod adını çıkar
        var lastSlash = action.LastIndexOf('/');
        return lastSlash >= 0 ? action[(lastSlash + 1)..] : action;
    }

    private async Task LogRequestAsync(WcfCorrelationState state, string requestBody)
    {
        try
        {
            await _logService.LogRequestAsync(new RequestLogEntry
            {
                HttpMethod = "SOAP",
                RequestPath = $"{_serviceName}/{GetActionName(state.Action)}",
                RequestBody = requestBody,
                ContentType = "application/soap+xml",
                QueryString = null,
                RequestHeaders = new Dictionary<string, string>
                {
                    ["SOAPAction"] = state.Action ?? "",
                    ["X-Correlation-ID"] = state.CorrelationId ?? ""
                }
            });
        }
        catch
        {
            // Loglama hatası ana akışı etkilememeli
        }
    }

    private async Task LogResponseAsync(WcfCorrelationState state, string responseBody, double duration, bool isFault)
    {
        try
        {
            await _logService.LogResponseAsync(new ResponseLogEntry
            {
                StatusCode = isFault ? 500 : 200,
                ResponseBody = responseBody,
                DurationMs = (long)duration,
                ContentType = "application/soap+xml"
            });

            // Performance log
            await _logService.LogPerformanceAsync(new PerformanceLogEntry
            {
                OperationName = $"{_serviceName}.{GetActionName(state.Action)}",
                OperationType = "WCF",
                DurationMs = (long)duration,
                Success = !isFault,
                Metadata = new Dictionary<string, object>
                {
                    ["ServiceName"] = _serviceName,
                    ["Action"] = state.Action ?? "",
                    ["IsFault"] = isFault
                }
            });
        }
        catch
        {
            // Loglama hatası ana akışı etkilememeli
        }
    }
}

/// <summary>
/// WCF request correlation state
/// </summary>
internal class WcfCorrelationState
{
    public DateTime StartTime { get; set; }
    public string? Action { get; set; }
    public string? CorrelationId { get; set; }
    public string? RequestBody { get; set; }
}

/// <summary>
/// WCF Endpoint Behavior - Inspector'ı ekler
/// </summary>
public class WcfLoggingBehavior : IEndpointBehavior
{
    private readonly ILogger _logger;
    private readonly ICorrelationContext _correlationContext;
    private readonly ILogService _logService;
    private readonly string _serviceName;

    public WcfLoggingBehavior(
        ILogger logger,
        ICorrelationContext correlationContext,
        ILogService logService,
        string serviceName)
    {
        _logger = logger;
        _correlationContext = correlationContext;
        _logService = logService;
        _serviceName = serviceName;
    }

    public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters) { }

    public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
    {
        clientRuntime.ClientMessageInspectors.Add(
            new WcfLoggingInspector(_logger, _correlationContext, _logService, _serviceName));
    }

    public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher) { }

    public void Validate(ServiceEndpoint endpoint) { }
}

