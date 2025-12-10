using Common.Networking;

namespace Server.Management.Transactions;

public interface IServerTransaction
{
    public TransactionId Id { get; }
    public u64 ReadyTick { get; }
    public u64 ExecuteTick { get; set; }

    /// <summary>
    /// Execute the transaction at the given tick. Should perform minimal work, as most preparation should be done in Prepare.
    /// </summary>
    public void Execute(u64 currentTick);

    /// <summary>
    /// prepare the transaction to be ready for execution. This may involve loading assets or other async work.
    /// <remarks>must set <see cref="ReadyTick"/> to the tick when the transaction will be ready</remarks>
    /// </summary>
    /// <returns></returns>
    public Task Prepare(Func<u64> tickAccessor);
}