namespace LeaderAnalytics.Vyntix.Web.Domain;

public class AsyncResult
{
    public bool Success { get; set; }
    public string ErrorMessage { get; set; }
}

public class AsyncResult<T> : AsyncResult
{
    public T Result { get; set; }
    public int TotalResultCount { get; set; }
}

public class RowOpResult
{
    public enum RowOp
    {
        Fail,
        Insert,
        Update,
        Delete
    }

    public RowOp Result { get; set; } = RowOp.Fail;
    public string ErrorMessage { get; set; }
}

public class RowOpResult<T> : RowOpResult
{
    public T Item { get; set; }

    public RowOpResult() { }

    public RowOpResult(T item)
    {
        this.Item = item;
    }
}

public class ValidationResult<T>
{
    public bool Success { get; set; }
    public string ErrorMessage { get; set; }
    public T ExistingEntity { get; set; }
}
