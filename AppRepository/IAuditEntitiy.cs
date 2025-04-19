namespace AppRepository
{
    public interface IAuditEntitiy
    {
        public DateTime Created { get; set; }
        public DateTime? Updated { get; set; }
    }
}
