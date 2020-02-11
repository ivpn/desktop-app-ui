namespace IVPN.Lib
{
    public delegate void OnOperationExecutionEventDelegate (IOperationStartStopNotifier sender);

    public interface IOperationStartStopNotifier 
    {
        event OnOperationExecutionEventDelegate OnWillExecute;
        event OnOperationExecutionEventDelegate OnDidExecute;
    }
}
