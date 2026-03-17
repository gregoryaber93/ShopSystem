namespace PromotionService.Application.Pipeline;

public interface IPipelineFilter<TContext>
{
    Task ExecuteAsync(TContext context, Func<Task> next, CancellationToken cancellationToken);
}
