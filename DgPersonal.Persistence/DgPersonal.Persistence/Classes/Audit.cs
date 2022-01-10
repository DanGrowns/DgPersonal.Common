using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DgPersonal.Persistence.Classes
{
    public class Audit
    {
        protected internal Audit(){}
        public Audit(string action, string type, string typeId, string previousValue, int changedBy)
        {
            Id = 0;
            EventDate = DateTime.Now;
            Action = action;
            Type = type;
            TypeId = typeId;
            PreviousValue = previousValue;
            ChangedBy = changedBy;
        }
        
        public int Id { get; }
        public DateTime EventDate { get; }
        public int ChangedBy { get; }
        public string Action { get; }
        public string Type { get; }
        public string TypeId { get; }
        public string PreviousValue { get; }
    }
    
    public class AuditConfiguration : IEntityTypeConfiguration<Audit>
    {
        private string Schema { get; }
        private string Table { get; }
        public AuditConfiguration(string schema, string tableName = "Audit")
        {
            Schema = schema;
            Table = tableName;
        }

        public void Configure(EntityTypeBuilder<Audit> builder)
        {
            builder.ToTable(Table, Schema);
            
            builder.HasIndex(x => x.Id).IsUnique();
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Action).IsRequired();
            builder.Property(x => x.Type).IsRequired();
            builder.Property(x => x.TypeId).IsRequired();
            builder.Property(x => x.PreviousValue).IsRequired();
            builder.Property(x => x.ChangedBy).IsRequired().HasDefaultValue(0);
            builder.Property(x => x.EventDate).IsRequired();
        }
    }
}