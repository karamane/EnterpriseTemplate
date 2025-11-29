using System.ServiceModel;
using Enterprise.Core.Application.Interfaces.Logging;
using Enterprise.Core.Application.Models.Logging;
using Enterprise.Core.Shared.ErrorCodes;
using Enterprise.Core.Shared.Exceptions;
using Microsoft.Extensions.Logging;

namespace Enterprise.Proxy.Core.Wcf;

/// <summary>
/// WCF Binding tipleri
/// </summary>
public enum WcfBindingType
{
    BasicHttp,
    NetTcp,
    WSHttp
}

/// <summary>
/// WCF Proxy base sınıfı
/// Otomatik loglama ve exception handling sağlar
/// </summary>
/// <typeparam name="TChannel">WCF Channel tipi</typeparam>
public abstract class WcfProxyBase<TChannel> : IDisposable where TChannel : class
{
    protected readonly ILogger Logger;
    protected readonly ICorrelationContext CorrelationContext;
    protected readonly ILogService LogService;
    protected readonly string ServiceName;
    protected readonly WcfBindingType BindingType;

    private ChannelFactory<TChannel>? _channelFactory;
    private TChannel? _channel;

    /// <summary>
    /// Constructor - serviceName ile (CreateChannel sonra çağrılmalı)
    /// </summary>
    protected WcfProxyBase(
        ILogger logger,
        ICorrelationContext correlationContext,
        ILogService logService,
        string serviceName)
    {
        Logger = logger;
        CorrelationContext = correlationContext;
        LogService = logService;
        ServiceName = serviceName;
        BindingType = WcfBindingType.BasicHttp;
    }

    /// <summary>
    /// Constructor - endpoint ve binding ile (Channel otomatik oluşturulur)
    /// </summary>
    protected WcfProxyBase(
        string endpointUrl,
        WcfBindingType bindingType,
        ILogger logger,
        ILogService logService,
        ICorrelationContext correlationContext)
    {
        Logger = logger;
        CorrelationContext = correlationContext;
        LogService = logService;
        ServiceName = typeof(TChannel).Name.TrimStart('I');
        BindingType = bindingType;

        // Channel'ı otomatik oluştur
        CreateChannel(endpointUrl);
    }

    /// <summary>
    /// WCF Channel'ı oluşturur ve logging behavior'ı ekler
    /// </summary>
    protected TChannel CreateChannel(string endpointUrl)
    {
        var binding = CreateBinding();
        var endpoint = new EndpointAddress(endpointUrl);

        _channelFactory = new ChannelFactory<TChannel>(binding, endpoint);

        // Logging behavior ekle
        _channelFactory.Endpoint.EndpointBehaviors.Add(
            new WcfLoggingBehavior(Logger, CorrelationContext, LogService, ServiceName));

        _channel = _channelFactory.CreateChannel();

        return _channel;
    }

    /// <summary>
    /// Binding oluşturur - BindingType'a göre
    /// </summary>
    protected virtual System.ServiceModel.Channels.Binding CreateBinding()
    {
        return BindingType switch
        {
            WcfBindingType.NetTcp => new NetTcpBinding
            {
                MaxReceivedMessageSize = 10485760,
                ReceiveTimeout = TimeSpan.FromMinutes(2),
                SendTimeout = TimeSpan.FromMinutes(2),
                OpenTimeout = TimeSpan.FromMinutes(1),
                CloseTimeout = TimeSpan.FromMinutes(1)
            },
            WcfBindingType.WSHttp => new WSHttpBinding
            {
                MaxReceivedMessageSize = 10485760,
                ReceiveTimeout = TimeSpan.FromMinutes(2),
                SendTimeout = TimeSpan.FromMinutes(2),
                OpenTimeout = TimeSpan.FromMinutes(1),
                CloseTimeout = TimeSpan.FromMinutes(1)
            },
            _ => new BasicHttpBinding
            {
                MaxReceivedMessageSize = 10485760,
                ReceiveTimeout = TimeSpan.FromMinutes(2),
                SendTimeout = TimeSpan.FromMinutes(2),
                OpenTimeout = TimeSpan.FromMinutes(1),
                CloseTimeout = TimeSpan.FromMinutes(1)
            }
        };
    }

    /// <summary>
    /// WCF operasyonunu güvenli şekilde çalıştırır
    /// Exception'ları loglar ve BusinessException'a çevirir
    /// </summary>
    protected async Task<TResult> ExecuteAsync<TResult>(
        Func<TChannel, Task<TResult>> operation,
        string operationName,
        ErrorCode? customErrorCode = null)
    {
        var startTime = DateTime.UtcNow;

        try
        {
            Logger.LogDebug(
                "[{CorrelationId}] WCF Call Start: {ServiceName}.{Operation}",
                CorrelationContext.CorrelationId,
                ServiceName,
                operationName);

            var result = await operation(_channel!);

            var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;

            Logger.LogInformation(
                "[{CorrelationId}] WCF Call Success: {ServiceName}.{Operation} | Duration: {Duration}ms",
                CorrelationContext.CorrelationId,
                ServiceName,
                operationName,
                duration);

            return result;
        }
        catch (FaultException ex)
        {
            await HandleExceptionAsync(ex, operationName, customErrorCode ?? CommonErrorCodes.ExternalServiceError);
            throw; // BusinessException throw edildi
        }
        catch (CommunicationException ex)
        {
            await HandleExceptionAsync(ex, operationName, CommonErrorCodes.ExternalServiceUnavailable);
            throw;
        }
        catch (TimeoutException ex)
        {
            await HandleExceptionAsync(ex, operationName, CommonErrorCodes.ExternalServiceTimeout);
            throw;
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(ex, operationName, customErrorCode ?? CommonErrorCodes.ExternalServiceError);
            throw;
        }
    }

    /// <summary>
    /// Void operasyon için wrapper
    /// </summary>
    protected async Task ExecuteAsync(
        Func<TChannel, Task> operation,
        string operationName,
        ErrorCode? customErrorCode = null)
    {
        await ExecuteAsync(async channel =>
        {
            await operation(channel);
            return true;
        }, operationName, customErrorCode);
    }

    /// <summary>
    /// Exception'ı loglar ve BusinessException'a çevirir
    /// Orijinal hatayı KAYBETMEZ
    /// </summary>
    private async Task HandleExceptionAsync(Exception ex, string operationName, ErrorCode errorCode)
    {
        // 1. Önce mevcut hatayı logla (KRİTİK - hata kaybedilmemeli)
        Logger.LogError(ex,
            "[{CorrelationId}] WCF Call Failed: {ServiceName}.{Operation} | Error: {ErrorMessage}",
            CorrelationContext.CorrelationId,
            ServiceName,
            operationName,
            ex.Message);

        // 2. Exception log entry oluştur
        await LogService.LogExceptionAsync(new ExceptionLogEntry
        {
            ExceptionType = ex.GetType().FullName,
            ExceptionMessage = ex.Message,
            StackTrace = ex.StackTrace,
            InnerExceptionMessage = ex.InnerException?.Message,
            InnerExceptionType = ex.InnerException?.GetType().FullName,
            MethodName = $"{ServiceName}.{operationName}",
            LayerName = "Proxy",
            Severity = errorCode.Severity.ToString(),
            ExceptionCategory = "ExternalService",
            AdditionalData = new Dictionary<string, object>
            {
                ["ServiceName"] = ServiceName,
                ["OperationName"] = operationName,
                ["ErrorCode"] = errorCode.Code,
                ["CorrelationId"] = CorrelationContext.CorrelationId ?? ""
            }
        });

        // 3. BusinessException fırlat (orijinal exception'ı koru)
        throw new BusinessException(
            errorCode,
            ex,
            new Dictionary<string, object>
            {
                ["ServiceName"] = ServiceName,
                ["OperationName"] = operationName,
                ["OriginalError"] = ex.Message
            });
    }

    public void Dispose()
    {
        try
        {
            if (_channel is IClientChannel clientChannel)
            {
                if (clientChannel.State == CommunicationState.Faulted)
                {
                    clientChannel.Abort();
                }
                else
                {
                    clientChannel.Close();
                }
            }

            _channelFactory?.Close();
        }
        catch
        {
            _channelFactory?.Abort();
        }

        GC.SuppressFinalize(this);
    }
}

