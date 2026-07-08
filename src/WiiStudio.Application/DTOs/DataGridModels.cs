namespace WiiStudio.Application.DTOs;

public enum DataGridState
{
    Loading,
    Empty,
    NoResults,
    Ready,
    Error,
    Refreshing,
    Selecting,
    BulkActionRunning
}

public enum DataGridColumnKind
{
    Text,
    Number,
    Status,
    Icon,
    Actions
}

public enum SortDirection
{
    None,
    Ascending,
    Descending
}

public sealed record DataGridColumn(
    string Id,
    string Title,
    DataGridColumnKind Kind,
    int Order,
    bool IsSortable = false,
    bool IsVisible = true);

public sealed record DataGridQuery(
    string? SearchTerm,
    IReadOnlyDictionary<string, string> Filters,
    string? SortColumn,
    SortDirection SortDirection,
    int Page,
    int PageSize);

public sealed record DataGridStatus(
    DataGridState State,
    int TotalItems,
    int SelectedItems,
    string? Message = null);
