using CartService.Application;
using Grpc.Core;
using Grpc.Core.Interceptors;

namespace CartService.API.Grpc;

internal sealed class GrpcExceptionInterceptor(ILogger<GrpcExceptionInterceptor> logger) : Interceptor
{
    public override Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request, ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation) =>
        HandleAsync(() => continuation(request, context));

    public override Task ServerStreamingServerHandler<TRequest, TResponse>(
        TRequest request, IServerStreamWriter<TResponse> responseStream,
        ServerCallContext context, ServerStreamingServerMethod<TRequest, TResponse> continuation) =>
        HandleAsync(() => continuation(request, responseStream, context));

    public override Task<TResponse> ClientStreamingServerHandler<TRequest, TResponse>(
        IAsyncStreamReader<TRequest> requestStream, ServerCallContext context,
        ClientStreamingServerMethod<TRequest, TResponse> continuation) =>
        HandleAsync(() => continuation(requestStream, context));

    public override Task DuplexStreamingServerHandler<TRequest, TResponse>(
        IAsyncStreamReader<TRequest> requestStream, IServerStreamWriter<TResponse> responseStream,
        ServerCallContext context, DuplexStreamingServerMethod<TRequest, TResponse> continuation) =>
        HandleAsync(() => continuation(requestStream, responseStream, context));

    private async Task<T> HandleAsync<T>(Func<Task<T>> func)
    {
        try
        {
            return await func();
        }
        catch (Exception ex)
        {
            throw HandleException(ex);
        }
    }

    private async Task HandleAsync(Func<Task> func)
    {
        try
        {
            await func();
        }
        catch (Exception ex)
        {
            throw HandleException(ex);
        }
    }

    private RpcException HandleException(Exception exception)
    {
        if (exception is RpcException rpcEx)
        {
            logger.LogWarning(rpcEx, "gRPC call failed with status {StatusCode}: {Message}", rpcEx.StatusCode, rpcEx.Message);
            return rpcEx;
        }
           

        var (status, logLevel) = exception switch
        {
            CartNotFoundException ex => (new Status(StatusCode.NotFound, ex.Message), LogLevel.Warning),

            CartItemNotAddedException ex => (new Status(StatusCode.FailedPrecondition, ex.Message), LogLevel.Warning),

            CartServiceException or ArgumentException => (new Status(StatusCode.InvalidArgument, exception.Message), LogLevel.Information),

            OperationCanceledException => (new Status(StatusCode.Cancelled, "Operation was cancelled"), LogLevel.Information),

            _ => (new Status(StatusCode.Internal, "Internal server error"), LogLevel.Error)
        };


        logger.Log(logLevel, exception, "gRPC call failed with status {StatusCode}: {Message}", status.StatusCode, status.Detail);
        return new RpcException(status);
    }
}
