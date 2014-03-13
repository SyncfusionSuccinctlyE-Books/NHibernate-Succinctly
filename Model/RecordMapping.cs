using NHibernate;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using NHibernate.Type;

namespace Succinctly.Model
{
	public class RecordMapping : ClassMapping<Record>
	{
		public RecordMapping()
		{
			this.Table("record");
			this.Lazy(false);
			//this.Where("deleted = 0");
			//this.SqlDelete("UPDATE record SET deleted = 1 WHERE record_id = ?");
			//this.SqlInsert("INSERT INTO record (version, name, parent_record_id, created_by, created_at, updated_by, updated_at, deleted, record_id) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?)");
			//this.SqlUpdate("UPDATE record SET version = ?, name = ?, parent_record_id = ?, updated_by = ?, updated_at = ?, deleted = ? WHERE record_id = ?");

			this.Id(x => x.RecordId, x =>
			{
				x.Column("record_id");
				x.Generator(Generators.HighLow);
			});

			this.Version(x => x.Version, x =>
			{
				x.Column(y =>
				{
					y.NotNullable(true);
					y.Name("version");
				});
			});
			
			this.Property(x => x.CreatedAt, x =>
			{
				x.Column("created_at");
				x.NotNullable(true);
				x.Update(false);
				x.Insert(true);
			});
	
			this.Property(x => x.CreatedBy, x =>
			{
				x.Column("created_by");
				x.NotNullable(true);
				x.Update(false);
				x.Insert(true);
			});

			this.Property(x => x.UpdatedAt, x =>
			{
				x.Column("updated_at");
				x.NotNullable(true);
			});

			this.Property(x => x.UpdatedBy, x =>
			{
				x.Column("updated_by");
				x.NotNullable(true);
			});

			this.Property(x => x.Name, x =>
			{
				x.Column("name");
				x.Length(50);
				x.NotNullable(true);
			});

			this.Property(x => x.Deleted, x =>
			{
				x.Column("deleted");
				x.NotNullable(true);
			});

			this.ManyToOne(x => x.Parent, x =>
			{				
				x.Column("parent_record_id");
				x.NotNullable(false);
				x.Lazy(LazyRelation.NoProxy);
				x.Cascade(Cascade.None);
			});

			this.Bag(x => x.Children, x =>
			{
				x.Inverse(true);
				x.Cascade(Cascade.All | Cascade.DeleteOrphans);
				//x.SqlDelete("UPDATE record SET deleted = 1 WHERE record_id = ?");
				//x.SqlDeleteAll("UPDATE record SET deleted = 1 WHERE parent_record_id = ?");
				//x.Where("deleted = 0");
				x.Key(y =>
				{
					y.Column("parent_record_id");
					y.NotNullable(false);
				});
			}, x =>
			{
				x.OneToMany();
			});
		}
	}
}