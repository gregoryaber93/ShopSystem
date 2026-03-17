namespace PromotionService.Application.Pipeline;

public sealed class PipelineRunner<TContext>(IEnumerable<IPipelineFilter<TContext>> filters)
{
    private readonly IReadOnlyList<IPipelineFilter<TContext>> _filters = filters.ToList();

    public Task RunAsync(TContext context, CancellationToken cancellationToken)
        => ExecuteAsync(0, context, cancellationToken);

    private async Task ExecuteAsync(int index, TContext context, CancellationToken cancellationToken)
    {
        if (index >= _filters.Count)
            return;

        await _filters[index].ExecuteAsync(
            context,
            () => ExecuteAsync(index + 1, context, cancellationToken),
            cancellationToken);
    }
}
