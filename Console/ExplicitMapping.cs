using System.Drawing.Imaging;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Mapping.ByCode;
using NHibernate.Type;
using Succintly.Common;
using Succintly.Model;

namespace Succintly.Console
{
	public sealed class ExplicitMapping : BaseMapping
	{
		public override Configuration Map(Configuration cfg)
		{			
			ModelMapper mapper = new ModelMapper();

			mapper.Class<Address>(ca =>
			{
				ca.Table("address");
				ca.Lazy(true);
				ca.Id(x => x.CustomerId, map =>
				{
					map.Column("customer_id");
					map.Generator(Generators.Foreign<Address>(x => x.Customer));
				});
				ca.Property(x => x.City, x =>
				{
					x.NotNullable(true);
					x.Column("city");
					x.Length(50);
				});
				ca.Property(x => x.Country, x =>
				{
					x.NotNullable(true);
					x.Column("country");
					x.Length(50);
				});
				ca.Property(x => x.Street, x =>
				{
					x.NotNullable(true);
					x.Column("street");
					x.Length(50);
				});
				ca.Property(x => x.ZipCode, x =>
				{
					x.NotNullable(true);
					x.Column("zipcode");
					x.Length(10);
				});
				ca.OneToOne(x => x.Customer, x =>
				{
					x.Constrained(true);
				});
			});

			mapper.Class<Customer>(ca =>
			{
				ca.Table("customer");
				ca.Lazy(true);
				ca.Id(x => x.CustomerId, map =>
				{
					map.Column("customer_id");
					map.Generator(Generators.HighLow);
				});
				ca.NaturalId(x => x.Property(c => c.Name, y =>
				{
					y.Column("name");
					y.Length(50);
					y.NotNullable(true);
				}));
				ca.NaturalId(x => x.Property(c => c.Email, y =>
				{
					y.Column("email");
					y.Length(50);
				}));
				ca.Property(x => x.Name, p =>
				{
					p.NotNullable(true);
					p.Column("name");
				});					
				ca.Set(c => c.Orders, c =>
				{
					c.Key(x => x.Column("customer_id"));
					c.Fetch(CollectionFetchMode.Select);
					c.Inverse(true);
					c.Lazy(CollectionLazy.Extra);
					c.Cascade(Cascade.All | Cascade.DeleteOrphans);
				}, c => c.OneToMany());
				ca.Set(c => c.RecentOrders, c =>
				{
					c.Key(x => x.Column("customer_id"));
					c.Fetch(CollectionFetchMode.Select);
					c.Where("(date >= (GETDATE() - 7))");
					c.Inverse(true);
					c.Mutable(false);
					c.Cascade(Cascade.None);
					c.Lazy(CollectionLazy.Lazy);
				}, c => c.OneToMany());
				ca.OneToOne(x => x.Address, x =>
				{
					x.Cascade(Cascade.All | Cascade.DeleteOrphans);
					x.Constrained(false);
				});
			});
			
			mapper.Class<Order>(ca =>
			{
				ca.Table("order");
				ca.Lazy(true);
				ca.Id(x => x.OrderId, map =>
				{
					map.Column("order_id");
					map.Generator(Generators.HighLow);
				});
				ca.Property(x => x.State, x =>
				{
					x.NotNullable(true);
					x.Column("state");
					x.Type<EnumType<OrderState>>();
				});
				ca.Property(x => x.Date, x =>
				{
					x.NotNullable(true);
					x.Column("date");
				});
				ca.ManyToOne(c => c.Customer, a =>
				{
					a.Column("customer_id");
					a.NotNullable(true);
					a.Fetch(FetchKind.Select);
					a.Lazy(LazyRelation.NoProxy);
					a.Cascade(Cascade.All);
				});
				ca.Set(x => x.Details, x =>
				{
					x.Key(c => c.Column("order_id"));
					x.Fetch(CollectionFetchMode.Subselect);
					x.Inverse(true);
					x.Lazy(CollectionLazy.Extra);
					x.Cascade(Cascade.All | Cascade.DeleteOrphans);
				}, c => c.OneToMany());
			});

			mapper.Class<OrderDetail>(ca =>
			{
				ca.Table("order_detail");
				ca.Lazy(true);
				ca.Id(x => x.OrderDetailId, map =>
				{
					map.Column("order_detail_id");
					map.Generator(Generators.HighLow);
				});
				ca.Property(x => x.Quantity, x =>
				{
					x.NotNullable(true);
					x.Column("quantity");
				});
				ca.Property(x => x.ItemsPrice, x =>
				{
					x.Formula("(quantity * (SELECT product.price FROM product WHERE product.product_id = product_id))");
					x.Access(Accessor.None);
					x.Update(false);
					x.Type(NHibernateUtil.Decimal);
					x.Insert(false);
				});
				ca.ManyToOne(x => x.Order, x =>
				{
					x.NotNullable(true);
					x.Column("order_id");
					x.Fetch(FetchKind.Select);
					x.Lazy(LazyRelation.NoProxy);
					x.Cascade(Cascade.None);
				});
				ca.ManyToOne(x => x.Product, x =>
				{
					x.NotNullable(true);
					x.Column("product_id");
					x.Fetch(FetchKind.Select);
					x.Lazy(LazyRelation.NoProxy);
					x.Cascade(Cascade.None);
				});
			});

			mapper.Class<Product>(ca =>
			{
				ca.Table("product");
				ca.Lazy(true);
				ca.Id(x => x.ProductId, map =>
				{
					map.Column("product_id");
					map.Generator(Generators.HighLow);
				});
				ca.NaturalId(x => x.Property(c => c.Name));
				ca.Property(x => x.OrderCount, x =>
				{
					x.Formula("(SELECT COUNT(1) FROM order_detail WHERE order_detail.product_id = product_id)");
				});
				ca.Property(x => x.Name, x =>
				{
					x.NotNullable(true);
					x.Column("name");
				});
				ca.Property(x => x.Price, x =>
				{
					x.NotNullable(true);
					x.Column("price");
				});
				ca.Property(x => x.Picture, x =>
				{
					x.NotNullable(false);
					x.Column("picture");
					x.Lazy(true);
					x.Type<ImageUserType>(new { ImageFormat = ImageFormat.Gif });
				});
				ca.Property(x => x.Specification, x =>
				{
					x.NotNullable(true);
					x.Column("specification");
					x.Lazy(true);
					x.Type<XDocType>();
				});
				ca.Set(x => x.OrderDetails, x =>
				{
					x.Key(c => c.Column("product_id"));
					x.Inverse(true);
					x.Cascade(Cascade.All | Cascade.DeleteOrphans);
					x.Lazy(CollectionLazy.Lazy);
				}, c => c.OneToMany());
				ca.Map(x => x.Attributes, c =>
				{
					c.Cascade(Cascade.All);
					c.Lazy(CollectionLazy.Extra);
					c.Table("product_attribute");
					c.Key(y =>
					{
						y.Column("product_id");
						y.NotNullable(true);
					});
				}, k =>
				{
					k.Element(e =>
					{
						e.Column(y =>
						{
							y.Name("name");
							y.NotNullable(true);
						});
					});
				}, r =>
				{
					r.Element(e =>
					{
						e.Column("value");
						e.NotNullable(true);
					});
				});
			});

			mapper.AddMapping<PersonMapping>();
			mapper.AddMapping<ForeignCitizenMapping>();
			mapper.AddMapping<NationalCitizenMappping>();

			mapper.AddMapping<LanguageMapping>();
			mapper.AddMapping<TermMapping>();
			mapper.AddMapping<TranslationMapping>();

			mapper.AddMapping<RecordMapping>();

			//for mapping by code
			mapper.AddMapping<BlogMapping>();
			mapper.AddMapping<UserMapping>();
			mapper.AddMapping<PostMapping>();
			mapper.AddMapping<CommentMapping>();
			mapper.AddMapping<AttachmentMapping>();

			//mapper.AddMappings(typeof(BlogMapping).Assembly.GetTypes().Where(x => x.BaseType.IsGenericType && x.BaseType.GetGenericTypeDefinition() == typeof(ClassMapping<>)));

			HbmMapping mappings = mapper.CompileMappingForAllExplicitlyAddedEntities();

			cfg.AddDeserializedMapping(mappings, null);

			return (cfg);
		}
	}
}
