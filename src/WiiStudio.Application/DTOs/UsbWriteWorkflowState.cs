namespace WiiStudio.Application.DTOs;

public enum UsbWriteWorkflowState
{
    SelectingSource,
    SelectingTarget,
    Validating,
    ReadyForConfirmation,
    Confirmed,
    Running,
    Completed,
    Failed,
    Cancelled,
    Blocked
}
