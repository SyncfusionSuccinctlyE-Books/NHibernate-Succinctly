using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Transactions;
using System.Xml.Linq;
using log4net.Config;
using NHibernate;
using NHibernate.AdoNet;
using NHibernate.Cfg;
using NHibernate.Context;
using NHibernate.Dialect;
using NHibernate.Driver;
using NHibernate.Engine;
using NHibernate.Event;
using NHibernate.Impl;
using NHibernate.Linq;
using NHibernate.Mapping;
using NHibernate.Persister.Entity;
using NHibernate.Tool.hbm2ddl;
using NHibernate.Type;
using NHibernate.Validator.Cfg;
using NHibernate.Validator.Cfg.Loquacious;
using NHibernate.Validator.Engine;
using NHibernate.Validator.Event;
using NHibernate.Validator.Exceptions;
using Succintly.Common;
using Succintly.Model;

namespace Succintly.Console
{
	class Program
	{
		static Configuration BuildLoquaciousConventionsConfiguration()
		{			
			BaseConfiguration config = new LoquaciousConfiguration();
			Configuration cfg = config
			.BuildConfiguration<Sql2008ClientDriver, MsSql2008Dialect, ThreadStaticSessionContext, SqlClientBatchingBatcherFactory>(new ConventionMapping(typeof(Blog).Assembly.FullName), "Succintly", false);

			return (cfg);
		}

		static Configuration BuildFileConventionsConfiguration()
		{
			BaseConfiguration config = new Succintly.Common.XmlConfiguration();
			Configuration cfg = config
			.BuildConfiguration(null, null, null, null, new ConventionMapping(typeof(Blog).Assembly.FullName), "Succintly", false);

			return (cfg);
		}

		static Configuration BuildLoquaciousExplicitConfiguration()
		{
			BaseConfiguration config = new LoquaciousConfiguration();
			Configuration cfg = config
			.BuildConfiguration<Sql2008ClientDriver, MsSql2008Dialect, ThreadStaticSessionContext, SqlClientBatchingBatcherFactory>(new CompositeMapping(/*new ConventionMapping(typeof(Blog).Assembly),*//*new XmlMapping(typeof(Blog).Assembly), */new ExplicitMapping()/*, new AttributeMapping(typeof(Blog))*/), "Succintly", true);

			return (cfg);
		}

		static void NamedQueries(Configuration cfg)
		{
			//named HQL query
			cfg.NamedQueries["ReceivedOrders"] = new NamedQueryDefinition("from Order o where o.State = 0", true, null, 20, 0, FlushMode.Never, true, "Received orders", null);
			cfg.NamedQueries["LastNOrders"] = new NamedQueryDefinition("from Order o order by o.Date desc take :orders", true, null, 0, 0, FlushMode.Never, true, "Last N orders", new Dictionary<String, String>() { { "orders", typeof(Int32).Name } });

			//named SQL query
			cfg.NamedSQLQueries["GetMostExpensiveNOrders"] = new NamedSQLQueryDefinition("SELECT TOP (:orders) {o.*}, product.price * orderdetail.count order_price " +
			"FROM orderdetail " +
			"INNER JOIN product " +
			"ON orderdetail.product_id = product.product_id " +
			"INNER JOIN [order] o " +
			"ON o.order_id = orderdetail.order_id " +
			"ORDER BY order_price DESC", new NHibernate.Engine.Query.Sql.INativeSQLQueryReturn[] { new NHibernate.Engine.Query.Sql.NativeSQLQueryRootReturn("o", typeof(Order).FullName, LockMode.None) }, new List<String>(), true, null, 20, 0, FlushMode.Never, null, true, "Get the most expensive N orders", new Dictionary<String, String>() { { "orders", typeof(Int32).Name } }, true);

			using (ISessionFactory sessionFactory = cfg.BuildSessionFactory())
			using (ISession session = sessionFactory.OpenSession())
			{
				//calling a named query without parameters
				var receivedOrders = session.GetNamedQuery("ReceivedOrders").List<Order>();

				//calling a named SQL query with parameters
				var mostExpensiveNOrders = session.GetNamedQuery("GetMostExpensiveNOrders").SetParameter("orders", 5).List<Order>();

				//calling a named query with parameters
				var lastNOrders = session.GetNamedQuery("LastNOrders").SetParameter("orders", 10).List<Order>();
			}
		}

		static void Validation(Configuration cfg)
		{
			//NHibernate Validator
			//loquacious configuration
			FluentConfiguration validatorConfiguration = new FluentConfiguration();
			validatorConfiguration/*.Register(new CustomerValidation())*/
			.Register(new Type[] { typeof(Customer) })
			.SetDefaultValidatorMode(ValidatorMode.UseExternal)
			.IntegrateWithNHibernate
			.ApplyingDDLConstraints()
			.RegisteringListeners();

			//xml configuration
			//NHibernate.Validator.Cfg.XmlConfiguration validatorConfiguration = new NHibernate.Validator.Cfg.XmlConfiguration();

			NHibernate.Validator.Cfg.Environment.SharedEngineProvider = new NHibernateSharedEngineProvider();
			ValidatorEngine validatorEngine = NHibernate.Validator.Cfg.Environment.SharedEngineProvider.GetEngine();
			validatorEngine.Configure(validatorConfiguration);

			cfg.Initialize(validatorEngine);

			using (ISessionFactory sessionFactory = cfg.BuildSessionFactory())
			using (ISession session = sessionFactory.OpenSession())
			using (session.BeginTransaction())
			{
				Customer c = new Customer() { CustomerId = 1, Name = "a", Address = new Address() { Country = "Portugal", City = "Coimbra", Street = "Rua", ZipCode = "3000" }, Email = "c" };

				InvalidValue[] invalidValuesObtainedExplicitly = validatorEngine.Validate(c);

				try
				{
					session.Save(c);
					session.Flush();
				}
				catch (InvalidStateException ex)
				{
					InvalidValue[] invalidValuesObtainedFromException = ex.GetInvalidValues();
				}
			}
		}

		static void Filters(Configuration cfg)
		{
			cfg.AddFilterDefinition(new NHibernate.Engine.FilterDefinition("CurrentLanguage", "language_id = :code", new Dictionary<String, IType>() { { "code", NHibernateUtil.String } }, false));

			cfg.GetClassMapping(typeof(Translation)).AddFilter("CurrentLanguage", "language_id = :code");
			cfg.GetCollectionMapping(String.Concat(typeof(Term).FullName, ".Translations")).AddFilter("CurrentLanguage", "language_id = :code");

			using (ISessionFactory sessionFactory = cfg.BuildSessionFactory())
			using (ISession session = sessionFactory.OpenSession())
			{
				Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("pt-pt");
				session.EnableFilter("CurrentLanguage").SetParameter("code", Thread.CurrentThread.CurrentCulture.Name);

				var term = session.Query<Term>().FirstOrDefault();
				var translations = term.Translations.ToList();
				var translation = term.Translations.First();
				var language = translation.TranslationId.Language;

				session.DisableFilter("CurrentLanguage");
			}

			cfg.GetClassMapping(typeof(Translation)).FilterMap.Remove("CurrentLanguage");
			cfg.GetCollectionMapping(String.Concat(typeof(Term).FullName, ".Translations")).FilterMap.Remove("CurrentLanguage");

			cfg.FilterDefinitions.Remove("CurrentLanguage");
		}

		static void Auditing(Configuration cfg)
		{
			AuditableListener listener = new AuditableListener();
			listener.Register(cfg);

			Record r = null;

			using (ISessionFactory sessionFactory = cfg.BuildSessionFactory())
			using (ISession session = sessionFactory.OpenSession())
			{
				Record parent = new Record();
				parent.Name = "Parent";

				Record child = new Record();
				child.Name = "Child";
				child.Parent = parent;

				parent.Children.Add(child);

				session.Save(parent);
				session.Flush();

				r = parent;
			}

			using (ISessionFactory sessionFactory = cfg.BuildSessionFactory())
			using (ISession session = sessionFactory.OpenSession())
			{
				r.Name = "Parent Changed";
				r = session.Merge<Record>(r);

				session.Flush();
			}

			listener.Unregister(cfg);
		}

		static void GenerateSchema(Configuration cfg)
		{
			SchemaExport export = new SchemaExport(cfg)
			.SetOutputFile("Succintly.sql");

			export.Execute(true, true, false);
		}

		static void Interceptor(Configuration cfg)
		{
			cfg.SetInterceptor(new SendSqlInterceptor(() => "SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED"));

			using (ISessionFactory sessionFactory = cfg.BuildSessionFactory())
			using (ISession session = sessionFactory.OpenSession())
			{
				Product p = session.Query<Product>().First();
			}

			cfg.SetInterceptor(new NotifyPropertyChangedInterceptor());

			using (ISessionFactory sessionFactory = cfg.BuildSessionFactory())
			using (ISession session = sessionFactory.OpenSession())
			using (session.BeginTransaction())
			{
				Product p = session.Query<Product>().First();
				INotifyPropertyChanged npc = p as INotifyPropertyChanged;
				npc.PropertyChanged += delegate(Object sender, PropertyChangedEventArgs args)
				{
					args.ToString();
				};

				p.Price *= 10;
			}

			cfg.SetInterceptor(new EmptyInterceptor());
		}

		static void Loader(Configuration cfg)
		{
			cfg.NamedSQLQueries["LoadRecord"] = new NamedSQLQueryDefinition("SELECT {r.*} FROM record r WHERE r.record_id = ? AND r.deleted = 0", new NHibernate.Engine.Query.Sql.INativeSQLQueryReturn[] { new NHibernate.Engine.Query.Sql.NativeSQLQueryRootReturn("r", typeof(Record).FullName, LockMode.None) }, new List<String>(), true, null, 20, 0, FlushMode.Never, null, true, "Load a record", new Dictionary<String, String>(), true);
			cfg.NamedSQLQueries["LoadRecordChildren"] = new NamedSQLQueryDefinition("SELECT {r.*} FROM record r WHERE r.parent_record_id = ? AND r.deleted = 0", new NHibernate.Engine.Query.Sql.INativeSQLQueryReturn[] { new NHibernate.Engine.Query.Sql.NativeSQLQueryCollectionReturn("r", typeof(Record).FullName, "Children", new Dictionary<String, String[]>(), LockMode.None) }, new List<String>(), true, null, 20, 0, FlushMode.Never, null, true, "Load a record's children", new Dictionary<String, String>(), true);

			PersistentClass classMapping = cfg.GetClassMapping(typeof(Record).FullName);
			classMapping.LoaderName = "LoadRecord";

			Collection collectionMapping = cfg.GetCollectionMapping(String.Concat(typeof(Record).FullName, ".Children"));
			collectionMapping.LoaderName = "LoadRecordChildren";

			using (ISessionFactory sessionFactory = cfg.BuildSessionFactory())
			using (ISession session = sessionFactory.OpenSession())
			{
				Record r = session.Get<Record>(1703936);

				r.Children.ToList();
			}

			classMapping.LoaderName = null;
			collectionMapping.LoaderName = null;

			cfg.NamedSQLQueries.Remove("LoadRecordChildren");
			cfg.NamedSQLQueries.Remove("LoadRecord");
		}

		static void Restrictions(Configuration cfg)
		{
			PersistentClass classMapping = cfg.GetClassMapping(typeof(Record).FullName);
			classMapping.Where = "deleted = 0";

			Collection collectionMapping = cfg.GetCollectionMapping(String.Concat(typeof(Record).FullName, ".Children"));
			collectionMapping.Where = "deleted = 0";

			using (ISessionFactory sessionFactory = cfg.BuildSessionFactory())
			using (ISession session = sessionFactory.OpenSession())
			using (session.BeginTransaction())
			{
				Record r = session.Query<Record>().FirstOrDefault();
				IEnumerable<Record> children = r.Children.ToList();
			}

			classMapping.Where = null;
			collectionMapping.Where = null;
		}
		
		static void SoftDeletes(Configuration cfg)
		{
			PersistentClass classMapping = cfg.GetClassMapping(typeof(Record).FullName);
			classMapping.SetCustomSQLDelete("UPDATE record SET deleted = 1 WHERE record_id = ?", true, ExecuteUpdateResultCheckStyle.Count);

			Collection collectionMapping = cfg.GetCollectionMapping(String.Concat(typeof(Record).FullName, ".Children"));
			collectionMapping.SetCustomSQLDelete("UPDATE record SET deleted = 1 WHERE record_id = ?", true, ExecuteUpdateResultCheckStyle.Count);
			collectionMapping.SetCustomSQLDeleteAll("UPDATE record SET deleted = 1 WHERE parent_record_id = ?", true, ExecuteUpdateResultCheckStyle.Count);

			using (ISessionFactory sessionFactory = cfg.BuildSessionFactory())
			using (ISession session = sessionFactory.OpenSession())
			using (session.BeginTransaction())
			{
				Record r = session.Query<Record>().FirstOrDefault();

				session.Delete(r);

				session.Flush();
			}

			classMapping.SetCustomSQLDelete(String.Empty, false, ExecuteUpdateResultCheckStyle.None);
			collectionMapping.SetCustomSQLDeleteAll(String.Empty, false, ExecuteUpdateResultCheckStyle.None);
		}

		static void SqlOperations(Configuration cfg)
		{
			PersistentClass classMapping = cfg.GetClassMapping(typeof(Record).FullName);
			classMapping.SetCustomSQLInsert("INSERT INTO record (version, name, parent_record_id, created_by, created_at, updated_by, updated_at, deleted, record_id) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?)", true, ExecuteUpdateResultCheckStyle.Count);
			classMapping.SetCustomSQLUpdate("UPDATE record SET version = ?, name = ?, parent_record_id = ?, updated_by = ?, updated_at = ?, deleted = ? WHERE record_id = ?", true, ExecuteUpdateResultCheckStyle.Count);
			classMapping.SetCustomSQLDelete("UPDATE record SET deleted = 1 WHERE record_id = ?", true, ExecuteUpdateResultCheckStyle.Count);

			using (ISessionFactory sessionFactory = cfg.BuildSessionFactory())
			using (ISession session = sessionFactory.OpenSession())
			using (session.BeginTransaction())
			{
				Record r = new Record() { Name = "aaaa", CreatedAt = DateTime.Now, CreatedBy = "created", UpdatedAt = DateTime.Now.AddDays(1), UpdatedBy = "updated" };

				session.Save(r);

				session.Flush();

				r.Name += "_bbbbb";

				session.Flush();

				session.Delete(r);

				session.Flush();
			}

			classMapping.SetCustomSQLInsert(String.Empty, false, ExecuteUpdateResultCheckStyle.None);
			classMapping.SetCustomSQLUpdate(String.Empty, false, ExecuteUpdateResultCheckStyle.None);
			classMapping.SetCustomSQLDelete(String.Empty, false, ExecuteUpdateResultCheckStyle.None);
		}

		static void Statistics(Configuration cfg)
		{
			cfg.SetProperty(NHibernate.Cfg.Environment.GenerateStatistics, Boolean.TrueString);

			using (ISessionFactory sessionFactory = cfg.BuildSessionFactory())
			using (ISession session = sessionFactory.OpenSession())
			using (session.BeginTransaction())
			{
				IEnumerable<Product> products = session.Query<Product>().ToList();
				products.First().Price *= 10;
				products.First().OrderDetails.ToList();

				session.Flush();

				DateTime sessionFactoryCreationTime = sessionFactory.Statistics.StartTime;
				Int64 loadedEntitiesCount = sessionFactory.Statistics.EntityLoadCount;
				Int64 loadedCollectionsCount = sessionFactory.Statistics.CollectionLoadCount;
				Int64 updatedEntitiesCount = sessionFactory.Statistics.EntityUpdateCount;
				Int64 queryCount = sessionFactory.Statistics.QueryExecutionCount;
				Int64 connectCount = sessionFactory.Statistics.ConnectCount;

				sessionFactory.Statistics.Clear();
			}

			cfg.SetProperty(NHibernate.Cfg.Environment.GenerateStatistics, Boolean.FalseString);
		}

		static void Listeners(Configuration cfg)
		{
			ProductCreatedListener.ProductCreated += delegate(Product p)
			{
				p.ToString();
			};

			cfg.AppendListeners(NHibernate.Event.ListenerType.FlushEntity, new NHibernate.Event.IFlushEntityEventListener[]{ new ProductCreatedListener() });

			using (ISessionFactory sessionFactory = cfg.BuildSessionFactory())
			using (ISession session = sessionFactory.OpenSession())
			using (session.BeginTransaction())
			{
				Product product = new Product() { Name = "Some Product", Price = 100, Specification = XDocument.Parse("<data/>") };

				session.Save(product);
				session.Flush();
			}

			cfg.SetListener(NHibernate.Event.ListenerType.FlushEntity, new NHibernate.Event.Default.DefaultFlushEntityEventListener());
		}

		static void Transactions(ISessionFactory sessionFactory)
		{
			using (TransactionScope tx = new TransactionScope())
			{
				using (ISession session1 = sessionFactory.OpenSession())
				{
					session1.FlushMode = FlushMode.Commit;

					Product p = new Product() { Name = "Some Product", Price = 5, Specification = XDocument.Parse("<data/>") };

					session1.Save(p);
				}

				tx.Complete();
			}

			using (TransactionScope tx = new TransactionScope())
			{
				using (ISession session2 = sessionFactory.OpenSession())
				{
					session2.FlushMode = FlushMode.Commit;

					Product p = session2.Query<Product>().Where(x => x.Name == "Some Product").Single();

					session2.Delete(p);
				}

				tx.Complete();
			}
		}

		static void AddItems(ISessionFactory sessionFactory)
		{
			using (ISession session = sessionFactory.OpenSession())
			{
				Language pt = new Language() { Name = "Portuguese (Portugal)", LanguageId = "pt-pt" };
				Language en = new Language() { Name = "English (United States)", LanguageId = "en-us" };

				Term someTerm = new Term() { Description = "This is a simple term" };
				someTerm.Translations.Add(new Translation() { Text = "Uma Tradução", TranslationId = new LanguageTerm() { Language = pt, Term = someTerm } });
				someTerm.Translations.Add(new Translation() { Text = "Some Translation", TranslationId = new LanguageTerm() { Language = en, Term = someTerm } });

				session.Save(pt);
				session.Save(en);

				session.Save(someTerm);
				
				session.Flush();
			}

			using (ISession session = sessionFactory.OpenSession())
			{
				User u = new User() { Birthday = new DateTime(1975, 8, 19), Username = "ricardoperes", Details = new UserDetail() { Email = "rjperes@hotmail.com", Fullname = "Ricardo Peres", Url = "http://weblogs.asp.net/ricardoperes" } };
				Blog b = new Blog() { Name = "Development With A Dot", Creation = new DateTime(2008, 8, 13, 10, 0, 0), Owner = u };
				Post p = new Post() { Blog = b, Title = "First Post", Timestamp = new DateTime(2008, 8, 13, 10, 5, 0), Content = "Greetings, everyone!" };
				Comment c = new Comment() { Post = p, Content = "Nice post!", Timestamp = new DateTime(2008, 8, 13, 10, 10, 0), Details = new UserDetail() { Email = "some@commenter.com", Fullname = "Some Commenter", Url = "" } };
				Attachment a = new Attachment() { Post = p, Contents = System.IO.File.ReadAllBytes("iphone.png"), Filename = "iPhone.png", Timestamp = DateTime.Now };

				u.Blogs.Add(b);

				b.Posts.Add(p);

				p.Tags.Add("General");
				p.Attachments.Add(a);
				p.Comments.Add(c);


				session.Save(u);
				session.Save(b);
				session.Save(p);

				session.Flush();
			}

			using (ISession session = sessionFactory.OpenSession())
			{
				NationalCitizen nc = new NationalCitizen();
				nc.Name = "Ricardo Peres";
				nc.NationalIdentityCard = "10602518";
				nc.Gender = Gender.Male;

				ForeignCitizen fc = new ForeignCitizen();
				fc.Name = "JP Harkin";
				fc.Gender = Gender.Male;
				fc.Passport = "ssadsadsa";
				fc.Country = "UK";

				session.Save(nc);
				session.Save(fc);

				session.Flush();
			}

			using (ISession session = sessionFactory.OpenSession())
			{
				Customer c = new Customer();
				c.Name = "Ricardo";
				c.Address = new Address() { Customer = c, Country = "Portugal", City = "Coimbra", Street = "Rua", ZipCode = "3000" };

				Product p = new Product();
				p.Name = "IPhone";
				p.Price = 100;
				p.Specification = XDocument.Parse("<specification><model>1</model><brand>2</brand><version>3</version></specification>");
				p.Picture = Image.FromFile("iphone.png");

				Order o = new Order();
				o.Customer = c;
				o.Date = DateTime.Today;
				o.State = OrderState.Received;

				OrderDetail od = new OrderDetail();
				od.Product = p;
				od.Quantity = 1;
				od.Order = o;

				o.Details.Add(od);

				session.Save(c);
				session.Save(p);
				session.Save(o);

				session.Flush();
			}
		}

		static void LinqQuerying(ISessionFactory sessionFactory)
		{
			using (ISession session = sessionFactory.OpenSession())
			using (session.BeginTransaction())
			{
				{
					//
					var ps = session.Query<Product>().Where(x => (session.Query<Customer>().Select(y => y.Name)).Contains(x.Name)).ToList();
				}

				{
					//projections
					var productsNameAndPrice = session.Query<Product>().Select(x => new { x.Price, x.Name }).ToList();
				}

				{
					//count
					var ordersCount = session.Query<Order>().Count();
				}

				{
					//fetch
					var ordersInLinq = session.Query<Order>().FetchMany(x => x.Details).ThenFetch(x => x.Product).ToList();
				}

				{
					//all
					var allPeople = session.Query<Person>().ToList();

					//all ForeignCitizens
					var allForeignCitizens = session.Query<ForeignCitizen>().ToList();
					var allForeignCitizensFromPerson = session.Query<Person>().Where(x => x is ForeignCitizen).ToList();
				}

				{
					//any
					var postsWithTag = session.Query<Post>().Where(x => x.Tags.Contains("asp.net")).ToList();
				}

				{
					//like
					var postsWithTitle = session.Query<Post>().Where(p => SqlMethods.Like(p.Title, "asp.net")).ToList();
				}

				{
					//filter on collection
					var ordersOfIphones = session.Query<OrderDetail>().Where(x => x.Product.Name == "iPhone").Select(x => x.Order).ToList();
				}

				{
					//two optional conditions
					var processedOrReceivedOrders = session.Query<Order>().Where(x => x.State == OrderState.Processed || x.State == OrderState.Received).ToList();
				}

				{
					//customers with two orders
					var customersWithTwoOrders = session.Query<Customer>().Where(x => x.Orders.Count() == 2).ToList();
				}

				{
					//set of values
					var orders = session.Query<Order>().Where(x => new OrderState[] { OrderState.Processed, OrderState.Sent }.Contains(x.State)).ToList();
				}

				{
					//parameters
					var recentOrders = session.Query<Order>().Where(x => x.Date <= DateTime.Today && x.Date > DateTime.Today.AddDays(-7)).ToList();
				}

				{
					//property navigation and sorting
					var firstCustomer = session.Query<OrderDetail>().OrderBy(x => x.Order.Date).Select(x => x.Order.Customer.Name).FirstOrDefault();
				}

				{
					//projections
					var productsAndOrderCount = session.Query<Product>().Select(x => new { x.Name, Count = x.OrderDetails.Count() }).ToList();
				}

				{
					//theta joins with projection
					var productCustomer = (from p in session.Query<Product>()
										   join od in session.Query<OrderDetail>() on p equals od.Product
										   select new { ProductName = p.Name, CustomerName = od.Order.Customer.Name }).ToList().Distinct();
				}

				{
					//grouping and counting
					var countByProduct = (from od in session.Query<OrderDetail>()
										  group od by od.Product.Name into p
										  select new { Name = p.Key, Count = p.Count() }).ToList();
				}

				{
					//first record that matches a condition
					var customer = session.Query<Customer>().OrderBy(x => x.Orders.Count()).FirstOrDefault();
				}

				{
					//multiple conditions
					var productsWithPriceBetween10And20 = session.Query<Product>().Where(x => x.Price >= 10 && x.Price < 20).ToList();
				}

				{
					//paging
					var productsFrom20to30 = session.Query<Product>().Skip(19).Take(10).ToList();
				}

				{
					//checking if a record exists
					var productsWithoutOrders = session.Query<Product>().Where(x => x.OrderDetails.Any()).ToList();
				}

				{

					//nesting queries
					var customersWithOrders = session.Query<Customer>().Where(x => x.Orders.Any());
					var ordersFromCustomers = session.Query<Order>().Where(x => customersWithOrders.Contains(x.Customer)).ToList();
				}

				{
					//grouping and summing
					var countByProduct = (from od in session.Query<OrderDetail>()
										  group od by od.Product.Name into p
										  select new { Product = p.Key, Count = p.Count() }).ToList();


				}
			}
		}

		static void CriteriaQuerying(ISessionFactory sessionFactory)
		{
			using (ISession session = sessionFactory.OpenSession())
			using (session.BeginTransaction())
			{
				{
					NHibernate.Criterion.ProjectionList list = NHibernate.Criterion.Projections.ProjectionList()
					.Add(NHibernate.Criterion.Projections.Property("Name"))
					.Add(NHibernate.Criterion.Projections.SqlProjection("case when price < 100 then 1 else 2 end as pricerange", new String[] { "pricerange" }, new IType[] { NHibernateUtil.String }));

					//projection with sql
					var productNameAndPriceRange = session.CreateCriteria(typeof(Product)).SetProjection(new NHibernate.Criterion.IProjection[] { list }).List<Object[]>();
				}

				{
					//sql
					var productsByNameLike = session.CreateCriteria(typeof(Product))
						.Add(NHibernate.Criterion.Expression.Sql("Name LIKE ?", "iPhone", NHibernateUtil.String))
						.UniqueResult<Product>();
				}

				{
					//projections
					var productsNameAndPrice = session.CreateCriteria(typeof(Product))
						.SetProjection(NHibernate.Criterion.Projections.ProjectionList().Add(NHibernate.Criterion.Projections.Property("Name")).Add(NHibernate.Criterion.Projections.Property("Price")))
						.List<Object[]>();
				}

				{
					//count
					var ordersCount = session.CreateCriteria(typeof(Order))
						.SetProjection(NHibernate.Criterion.Projections.RowCount())
						.UniqueResult<Int32>();
				}

				{
					//any
					//queries on collections of elements are not supported with Criteria						
				}

				{
					//like
					var postsWithTitle = session.CreateCriteria(typeof(Post), "p")
						.Add(NHibernate.Criterion.Restrictions.Like(NHibernate.Criterion.Projections.Property("p.Title"), "asp.net")).List<Post>();
				}

				{
					//sql projection
					///TODO
				}

				{
					//filter on collection
					var ordersOfIphones = session.CreateCriteria(typeof(Order))
						.CreateCriteria("Details")
						.CreateCriteria("Product")
						.SetFetchMode("Details", FetchMode.Eager)
						.Add(NHibernate.Criterion.Restrictions.Eq(NHibernate.Criterion.Projections.Property("Name"), "iPhone"))
						.List<Order>();
				}

				{
					//multiple joins
					var orderDetailsWithProductsAndOrders = session.CreateCriteria(typeof(OrderDetail), "od")
						.CreateAlias("od.Order", "o")
						.CreateAlias("od.Product", "p")
						.CreateAlias("o.Customer", "c")
						.SetResultTransformer(NHibernate.Transform.Transformers.DistinctRootEntity)
					.List<OrderDetail>();
				}

				{
					//checking if a record exists
					var productsWithoutOrders = session.CreateCriteria(typeof(Product), "p")
						.Add(NHibernate.Criterion.Restrictions.IsEmpty("p.OrderDetails"))
						.List<Product>();
				}

				{
					//checking if a record exists with detached
					var productsWithoutOrdersWithDetached = NHibernate.Criterion.DetachedCriteria.For(typeof(Product), "p")
						.Add(NHibernate.Criterion.Restrictions.IsEmpty("p.OrderDetails"));

					var productsWithoutOrders = productsWithoutOrdersWithDetached.GetExecutableCriteria(session).List<Product>();
				}

				{
					//by example
					var productsWithSamePrice = session.CreateCriteria(typeof(Product)).Add(NHibernate.Criterion.Example.Create(new Product() { Price = 1000 })).List<Product>();
				}

				{
					//two optional conditions
					var processedOrReceivedOrders = session.CreateCriteria(typeof(Order))
					.Add(NHibernate.Criterion.Restrictions.Or(NHibernate.Criterion.Restrictions.Eq(NHibernate.Criterion.Projections.Property("State"), OrderState.Processed), NHibernate.Criterion.Restrictions.Eq(NHibernate.Criterion.Projections.Property("State"), OrderState.Received)))
					.List<Order>();
				}

				{
					//grouping and counting
					var projection = NHibernate.Criterion.Projections.ProjectionList()
					.Add(NHibernate.Criterion.Projections.GroupProperty("p.Name"))
					.Add(NHibernate.Criterion.Projections.Count("Product"));

					var countByProduct = session.CreateCriteria(typeof(OrderDetail), "od")
					.CreateAlias("od.Product", "p")
					.SetProjection(projection)
					.List();
				}

				{
					//customers with two orders
					var innerQuery = NHibernate.Criterion.DetachedCriteria.For(typeof(Customer))
					.CreateAlias("Orders", "o")
					.SetProjection(NHibernate.Criterion.Projections.ProjectionList()
					.Add(NHibernate.Criterion.Projections.RowCount()));

					var customersWithTwoOrders = session.CreateCriteria(typeof(Customer), "c").Add(NHibernate.Criterion.Subqueries.Eq(2, innerQuery)).List<Customer>();
				}

				{
					//nesting queries
					var innerQuery = NHibernate.Criterion.DetachedCriteria.For(typeof(Customer), "ic").Add(NHibernate.Criterion.Restrictions.IsNotEmpty("Orders")).SetProjection(NHibernate.Criterion.Projections.ProjectionList().Add(NHibernate.Criterion.Projections.Constant(1)));
					var ordersFromCustomers = session.CreateCriteria(typeof(Order), "o").Add(NHibernate.Criterion.Subqueries.Exists(innerQuery)).List<Order>();
				}

				{
					//paging
					var productsFrom20to30 = session.CreateCriteria(typeof(Product)).SetMaxResults(10).SetFirstResult(20).List<Product>();
				}

				{
					//theta joins not supported with Criteria
				}

				{
					//property navigation and sorting
					var firstCustomer = session.CreateCriteria(typeof(OrderDetail), "od")
						.CreateAlias("Order", "o")
						.CreateAlias("o.Customer", "c")
						.SetProjection(NHibernate.Criterion.Projections.Property("c.Name"))
						.AddOrder(NHibernate.Criterion.Order.Asc("o.Date")).SetMaxResults(1).UniqueResult<String>();
				}

				{
					//set of values
					var orders = session.CreateCriteria(typeof(Order)).Add(NHibernate.Criterion.Restrictions.In(NHibernate.Criterion.Projections.Property("State"), new Object[] { OrderState.Processed, OrderState.Sent })).List<Order>();
				}

				{
					//parameters
					var recentOrders = session.CreateCriteria(typeof(Order), "o").Add(NHibernate.Criterion.Restrictions.Between(NHibernate.Criterion.Projections.Property("Date"), DateTime.Today.AddDays(-7), DateTime.Today)).List<Order>();
				}
			}
		}

		static void QueryOverQuerying(ISessionFactory sessionFactory)
		{
			using (ISession session = sessionFactory.OpenSession())
			using (session.BeginTransaction())
			{
				{
					//projection with sql
					var exists = session.QueryOver<Product>().Where(x => x.ProductId == 0).Select(NHibernate.Criterion.Projections.SqlProjection("case when exists (select 1 from product where price < 100) then 1 else 0 end as [exists]", new String[] { "exists" }, new IType[] { NHibernateUtil.Boolean })).SingleOrDefault<Boolean>();
				}

				{
					//projection with sql
					var productNameAndPriceRange = session.QueryOver<Product>().Select(NHibernate.Criterion.Projections.Property<Product>(x => x.Name), NHibernate.Criterion.Projections.SqlProjection("case when price < 100 then 1 else 2 end as pricerange", new String[] { "pricerange" }, new IType[] { NHibernateUtil.String })).List<Object[]>();
				}

				{
					//sql
					var productsByNameLike = session.QueryOver<Product>()
						.Where(NHibernate.Criterion.Expression.Sql("Name LIKE ?", "iPhone", NHibernateUtil.String))
						.SingleOrDefault();
				}

				{
					//projection
					var productsNameAndPrice = session.QueryOver<Product>()
						.Select(x => x.Price, x => x.Name)
						.List<Object[]>();
				}

				{
					//count
					var ordersCount = session.QueryOver<Order>()
						.RowCount();
				}

				{
					//any
					//queries on collections of elements are not supported with Query Over						
				}

				{
					//like
					var postsWithTitle = session.QueryOver<Post>()
						.Where(NHibernate.Criterion.Restrictions.Like("Title", "asp.net"))
						.List<Post>();
				}

				{
					//filter on collection
					OrderDetail orderDetailAlias = null;
					Product productAlias = null;

					//filter on association by using Criteria
					var o = session.QueryOver<Order>()
					.JoinQueryOver(x => x.Details, () => orderDetailAlias)
					.RootCriteria					
					.CreateAlias("Customer", "c")
					.Add(NHibernate.Criterion.Restrictions.Eq(NHibernate.Criterion.Projections.Property("c.Name"), "Some Name"))
					.List<Customer>();


					var ordersOfIphones = session.QueryOver<Order>()
					.Fetch(x => x.Details).Eager
					.JoinQueryOver(x => x.Details, () => orderDetailAlias)
					.JoinQueryOver(x => x.Product, () => productAlias)
					.Where(x => x.Name == "iPhone")
					.List<Order>();
				}

				{
					//checking if a record exists
					var productsWithoutOrders = session.QueryOver<Product>()
						.WithSubquery.WhereExists(NHibernate.Criterion.QueryOver.Of<OrderDetail>()
						.Select(x => x.Product))
						.List<Product>();
				}

				{
					//two optional conditions
					var processedOrReceivedOrders = session.QueryOver<Order>()
					.Where(x => x.State == OrderState.Processed || x.State == OrderState.Received)
					.List();
				}

				{
					//grouping and counting
					Product productAlias = null;

					var projection = session.QueryOver<OrderDetail>()
					.JoinAlias(x => x.Product, () => productAlias)
					.SelectList(list => list
						.SelectGroup(x => productAlias.Name)
						.SelectCount(x => x.OrderDetailId))
					.List<Object[]>();
				}

				{
					//by example
					var productsWithSamePrice = session.QueryOver<Product>().Where(NHibernate.Criterion.Example.Create(new Product() { Price = 1000 }))
					.List();
				}

				{
					//multiple joins
					OrderDetail orderDetailAlias = null;
					Order orderAlias = null;
					Product productAlias = null;
					Customer customerAlias = null;

					var orderDetailsWithProductsAndOrders = session.QueryOver<OrderDetail>(() => orderDetailAlias)
					.JoinAlias(x => x.Order, () => orderAlias)
					.JoinAlias(x => x.Product, () => productAlias)
					.JoinAlias(x => x.Order.Customer, () => customerAlias)
					.TransformUsing(NHibernate.Transform.Transformers.DistinctRootEntity)
					.List();
				}

				{
					//checking if a record exists
					var productsWithoutOrders = session.QueryOver<Product>()
						.WhereRestrictionOn(x => x.OrderDetails).IsEmpty.List();
				}

				{
					//checking if a record exists with detached
					var productsWithoutOrdersWithDetached = NHibernate.Criterion.QueryOver.Of<Product>()
						.WhereRestrictionOn(x => x.OrderDetails).IsEmpty;

					var productsWithoutOrders = productsWithoutOrdersWithDetached.GetExecutableQueryOver(session).List();
				}

				{
					//by example
					var productsWithSamePrice = session.QueryOver<Product>().Where(NHibernate.Criterion.Example.Create(new Product() { Price = 1000 })).List();
				}

				{
					//two optional conditions
					var processedOrReceivedOrders = session.QueryOver<Order>()
					.Where(NHibernate.Criterion.Restrictions.Disjunction()
					.Add(NHibernate.Criterion.Restrictions.Where<Order>(x => x.State == OrderState.Processed))
					.Add(NHibernate.Criterion.Restrictions.Where<Order>(x => x.State == OrderState.Received)))
					.List();
				}

				{
					//paging
					var productsFrom20to30 = session.QueryOver<Product>().Skip(20).Take(10).List();
				}

				{
					//parameters
					var recentOrders = session.QueryOver<Order>()
					.Where(NHibernate.Criterion.Restrictions.Between(NHibernate.Criterion.Projections.Property<Order>(x => x.Date), DateTime.Today.AddDays(-7), DateTime.Today))
					.List();
				}

				{
					//set of values
					var orders = session.QueryOver<Order>().WhereRestrictionOn(x => x.State).IsIn(new Object[] { OrderState.Processed, OrderState.Sent }).List();
				}

				{
					//property navigation and sorting
					Order orderAlias = null;
					Customer customerAlias = null;

					var firstCustomer = session.QueryOver<OrderDetail>()
						.JoinAlias(x => x.Order, () => orderAlias)
						.JoinAlias(x => x.Order.Customer, () => customerAlias)
						.OrderBy(x => orderAlias.Date).Desc
						.Select(x => customerAlias.Name)
						.Take(1)
						.SingleOrDefault<String>();
				}

				{
					//theta joins with projection
					//not supported with Query Over
				}

				{
					//customers with two orders
					var innerQuery = NHibernate.Criterion.QueryOver.Of<Customer>().JoinQueryOver(x => x.Orders).ToRowCountQuery();
					
					var customersWithTwoOrders = session.QueryOver<Customer>().WithSubquery.WhereValue(2).Eq(innerQuery).List();
				}

				{
					//nesting queries
					var innerQuery = NHibernate.Criterion.QueryOver.Of<Customer>().WhereRestrictionOn(x => x.Orders).Not.IsEmpty.Select(x => 1);

					var ordersFromCustomers = session.QueryOver<Order>().WithSubquery.WhereExists(innerQuery).List();
				}
			}
		}

		static void HqlQuerying(ISessionFactory sessionFactory)
		{
			using (ISession session = sessionFactory.OpenSession())
			using (session.BeginTransaction())
			{
				{
					//projections
					var productsNameAndPrice = session.CreateQuery("select p.Name, p.Price from Product p").List<Object[]>();
				}

				{
					//count
					var ordersCount = session.CreateQuery("select count(o) from Order o").UniqueResult<Int64>();
				}

				{
					//all
					var allPeople = session.CreateQuery("from Person").List<Person>();

					//all ForeignCitizens
					var allForeignCitizens = session.CreateQuery("from ForeignCitizen").List<ForeignCitizen>();
					var allForeignCitizensFromPerson = session.CreateQuery("from Person p where p.class = Succintly.Model.ForeignCitizen").List<ForeignCitizen>();

				}

				{
					//fetching multiple levels
					var ordersInHql = session.CreateQuery("from Order o join fetch o.Details d join fetch d.Product").List<Order>();
				}

				{
					//any
					var postsWithTag = session.CreateQuery("from Post p where :tag = any elements (p.Tags)").SetParameter("tag", "asp.net").List<Post>();
				}

				{
					//like
					var postsWithTitle = session.CreateQuery("from Post p where p.Title like :title").SetParameter("title", "asp.net").List<Post>();
				}

				{
					//filter on collection
					var ordersOfIphones = session.CreateQuery("select o from Order o join o.Details od where od.Product.Name = :name").SetParameter("name", "iPhone").List<Order>();
				}

				//distinct root transformer
				{
					var orderDetailsWithProductsAndOrders = session.CreateQuery("from OrderDetail od join od.Order join od.Product").SetResultTransformer(NHibernate.Transform.Transformers.DistinctRootEntity).List<Product>();
				}

				{
					//two optional conditions
					var processedOrReceivedOrders = session.CreateQuery("from Order o where o.State = :processed or o.State = :received")
					.SetParameter("processed", OrderState.Processed)
					.SetParameter("received", OrderState.Received)
					.List<Order>();
				}

				{
					//theta joins with projection
					var productCustomer = session.CreateQuery("select distinct p.Name, od.Order.Customer.Name from Product p, OrderDetail od where od.Product = p").List();
				}

				{
					//checking if a record exists
					var productsWithoutOrders = session.CreateQuery("from Product x where not exists elements(x.OrderDetails)").List<Product>();
				}

				{
					//grouping and counting
					var countByProduct = session.CreateQuery("select od.Product.Name, count(od) from OrderDetail od group by od.Product.Name").List();
				}

				{
					//customers with two orders
					var customersWithTwoOrders = session.CreateQuery("from Customer c where c.Orders.size = 2").List<Customer>();
				}

				{
					//set of values
					var orders = session.CreateQuery("from Order o where o.State in (:states)").SetParameterList("states", new OrderState[] { OrderState.Processed, OrderState.Sent }).List<Order>();
				}

				{
					//parameters
					var recentOrders = session.CreateQuery("from Order o where o.Date between :today and :a_week_ago").SetParameter("today", DateTime.Today).SetParameter("a_week_ago", DateTime.Today.AddDays(-7)).List<Order>();
				}

				{
					//paging
					var productsFrom20to30 = session.CreateQuery("from Product skip 19 take 10").List<Product>();
				}

				{
					//nesting queries
					var ordersFromCustomers = session.CreateQuery("from Order o where o.Customer in (select c from Customer c where exists elements(c.Orders))").List<Order>();
				}

				{
					//property navigation and sorting
					var firstCustomer = session.CreateQuery("select x.Order.Customer.Name from OrderDetail x order by x.Order.Date take 1").UniqueResult<String>();
				}
			}
		}

		static void LinqQueryManipulation(ISessionFactory sessionFactory)
		{
			using (ISession session = sessionFactory.OpenSession())
			using (session.BeginTransaction())
			{
				Boolean someCondition = false;
				String propertyToSort = "Price";

				var allProductsQuery = session.Query<Product>();

				if (someCondition == true)
				{
					allProductsQuery = allProductsQuery.Where(x => x.Price > 1000);
				}
				else
				{
					allProductsQuery = allProductsQuery.Where(x => x.Price <= 1000);
				}

				allProductsQuery = allProductsQuery.Where(x => x.OrderCount > 0);

				if (propertyToSort == "Name")
				{
					allProductsQuery = allProductsQuery.OrderBy(x => x.Name);
				}
				else
				{
					//LINQ extension
					allProductsQuery = allProductsQuery.OrderBy(propertyToSort);
				}

				var allProducts = allProductsQuery.ToList();
			}
		}

		static void SqlQuerying(ISessionFactory sessionFactory)
		{
			using (ISession session = sessionFactory.OpenSession())
			using (session.BeginTransaction())
			{
				//parameters
				var productsWithPriceLowerThan100 = session.CreateSQLQuery("SELECT {p.*} FROM Product p WHERE p.price < :price").AddEntity("p", typeof(Product)).SetParameter("price", 100).List<Product>();

				//mapping columns to entities
				var products = session.CreateSQLQuery("SELECT {p.*} FROM Product p").AddEntity("p", typeof(Product)).List<Product>();

				var productsWithPriceLowerThan1000 = session.CreateSQLQuery("SELECT p.Name, p.Price FROM Product p WHERE p.Price < :price").SetParameter("price", 1000).List();
				var lastWeekOrderDates = session.CreateSQLQuery("SELECT o.Date FROM [Order] o WHERE o.Date > DATEADD(DAY, -7, GETDATE())").List();

				//paging
				var productsFrom10To20 = session.CreateSQLQuery("SELECT * FROM Product").SetFirstResult(10).SetMaxResults(10).List();
			}
		}

		static void ExecutableHql(ISessionFactory sessionFactory)
		{
			using (ISession session = sessionFactory.OpenSession())
			using (session.BeginTransaction())
			{
				//unconditional update
				Int32 updatedRecords = session.CreateQuery("update Product p set p.Price = p.Price * 2").ExecuteUpdate();
				//delete with parameter
				Int32 deletedRecords = session.CreateQuery("delete from Product p where p.Price = :price").SetParameter("price", 0).ExecuteUpdate();
				//insert based on existing records
				Int32 insertedRecords = session.CreateQuery("insert into Product (ProductId, Name, Price, Specification) select p.ProductId * 10, p.Name + ' copy', p.Price * 2, p.Specification from Product p where p.ProductId > :id").SetParameter("id", 10000).ExecuteUpdate();
				//delete from query
				Int32 deletedRecords2 = session.Delete("from Product p where p.Price > 1000");
			}
		}

		static void LazyLoading(ISessionFactory sessionFactory)
		{
			using (ISession session = sessionFactory.OpenSession())
			{
				//lazy load an entity - no query is sent to the database
				Order o = session.Load<Order>(262144);

				Boolean isOrderInitialized = NHibernateUtil.IsInitialized(o);

				if (isOrderInitialized == false)
				{
					NHibernateUtil.Initialize(o);
				}

				Customer c = o.Customer;
				Product p = o.Details.First().Product;
				Boolean isSpecificationInitialized = NHibernateUtil.IsPropertyInitialized(p, "Specification");

				if (isSpecificationInitialized == false)
				{
					p.Specification.ToString();
				}
			}
		}

		static void TwoSessions(ISessionFactory sessionFactory)
		{
			Product product = null;

			using (ISession session = sessionFactory.OpenSession())
			using (session.BeginTransaction())
			{
				product = session.Query<Product>().First();
			}

			using (ISession session = sessionFactory.OpenSession())
			using (session.BeginTransaction())
			{
				//without going to the database
				//IEntityPersister persister = session.GetSessionImplementation().GetEntityPersister(NHibernateProxyHelper.GuessClass(product).FullName, product);
				//Object[] fields = persister.GetPropertyValues(product, session.ActiveEntityMode);
				//Object id = persister.GetIdentifier(product, session.ActiveEntityMode);
				//Object version = persister.GetVersion(entity, session.ActiveEntityMode);
				//EntityEntry entry = session.GetSessionImplementation().PersistenceContext.AddEntry(product, Status.Loaded, fields, null, id, version, LockMode.None, true, persister, true, false);

				session.Attach(product);

				product.Price *= 10;

				session.Flush();
			}

			using (ISession session = sessionFactory.OpenSession())
			using (session.BeginTransaction())
			{
				//going to the database
				product = session.Merge(product);

				product.Price /= 10;

				session.Update(product);

				session.Flush();
			}
		}

		static void Context(ISessionFactory sessionFactory)
		{	
			using (ISession session = sessionFactory.OpenSession())
			using (session.BeginTransaction())
			{
				CurrentSessionContext.Bind(session);

				ISession s = sessionFactory.GetCurrentSession();

				CurrentSessionContext.Unbind(sessionFactory);
			}
		}

		static void Streaming(ISessionFactory sessionFactory)
		{
			using (IStatelessSession session = sessionFactory.OpenStatelessSession())
			{
				StreamingCollection<Product> col = new StreamingCollection<Product>(x => System.Console.WriteLine("Product name: {0}", x.Name));
				session.CreateCriteria<Product>().List(col);
			}
		}

		static void BulkInserts(ISessionFactory sessionFactory)
		{
			Stopwatch watch = new Stopwatch();
			watch.Start();

			using (ISession session = sessionFactory.OpenSession())
			using (session.BeginTransaction())
			{
				for (Int32 i = 0; i < 1000; ++i)
				{
					session.Save(new Product() { Name = String.Format("Product {0}", i), Price = (i + 1) * 10, Specification = XDocument.Parse("<data/>") });

					if ((i % 100) == 0)
					{
						session.Flush();
						session.Clear();
					}
				}

				session.Transaction.Commit();
			}

			Int64 time1 = watch.ElapsedMilliseconds;

			watch.Restart();

			using (IStatelessSession session = sessionFactory.OpenStatelessSession())
			using (session.BeginTransaction())
			{
				for (Int32 i = 0; i < 1000; ++i)
				{
					session.Insert(new Product() { Name = String.Format("Product {0}", i), Price = (i + 1) * 10, Specification = XDocument.Parse("<data/>") });
				}

				session.Transaction.Commit();
			}

			Int64 time2 = watch.ElapsedMilliseconds;
		}

		static void DetachedQueries(ISessionFactory sessionFactory)
		{
			using (ISession session = sessionFactory.OpenSession())
			{
				DetachedQuery detachedHqlQuery = new DetachedQuery("from Product");
				detachedHqlQuery.GetExecutableQuery(session).List<Product>();

				NHibernate.Criterion.DetachedCriteria detachedCriteriaQuery = NHibernate.Criterion.DetachedCriteria.For(typeof(Product));
				detachedCriteriaQuery.GetExecutableCriteria(session).List<Product>();

				NHibernate.Criterion.QueryOver<Product> detachedQueryOverQuery = NHibernate.Criterion.QueryOver.Of<Product>();
				detachedQueryOverQuery.GetExecutableQueryOver(session).List<Product>();

				IQueryable<Product> detachedLinqQuery = session.Query<Product>();
				detachedLinqQuery.ToList();
			}
		}

		static void CollectionFiltering(ISessionFactory sessionFactory)
		{
			using (ISession session = sessionFactory.OpenSession())
			{
				Product product = session.Query<Product>().FirstOrDefault();
				OrderDetail mostRecentOrder = session.CreateFilter(product.OrderDetails, "order by Order.Date desc").SetMaxResults(1).UniqueResult<OrderDetail>();
				OrderDetail orderOfACustomer = session.CreateFilter(product.OrderDetails, "where Order.Customer.Name = :name").SetParameter("name", "Ricardo").SetMaxResults(1).UniqueResult<OrderDetail>();
			}
		}

		static void Futures(ISessionFactory sessionFactory)
		{
			using (ISession session = sessionFactory.OpenSession())
			{
				//future queries
				IEnumerable<Product> futureProductsFromLinq = session.Query<Product>().ToFuture();
				IEnumerable<Order> futureFirstOrderFromHql = session.CreateQuery("from Order o order by o.Date desc take 1").Future<Order>();
				IEnumerable<Customer> futureCustomersFromQueryOver = session.QueryOver<Customer>().Future();

				//future values
				IFutureValue<Decimal> futureProductsPriceSumFromCriteria = session.CreateCriteria(typeof(Product)).SetProjection(NHibernate.Criterion.Projections.Sum(NHibernate.Criterion.Projections.Property("Price"))).FutureValue<Decimal>();
				IFutureValue<Int32> futurePostsCountFromQueryOver = session.QueryOver<Post>().ToRowCountQuery().FutureValue<Int32>();

				var products = futureProductsFromLinq.ToList();
				var firstOrder = futureFirstOrderFromHql.Single();
				var customers = futureCustomersFromQueryOver.ToList();

				var postsCount = futurePostsCountFromQueryOver.Value;
				var productsPriceSum = futureProductsPriceSumFromCriteria.Value;
			}
		}

		static void MultiQueries(ISessionFactory sessionFactory)
		{
			using (ISession session = sessionFactory.OpenSession())
			using (ITransaction tx = session.BeginTransaction())
			{
				IMultiQuery mq = session.CreateMultiQuery();
				mq = mq.Add("from Product p where p.Price < :price").SetParameter("price", 10000);
				mq = mq.Add("from Customer c");
				mq = mq.Add("select distinct o.Date from Order o");

				IList results = mq.List();

				IEnumerable<Product> products = (results[0] as IList).OfType<Product>();
				IEnumerable<Customer> customers = (results[1] as IList).OfType<Customer>();
				IEnumerable<DateTime> dates = (results[2] as IList).OfType<DateTime>();
			}

			using (ISession session = sessionFactory.OpenSession())
			using (ITransaction tx = session.BeginTransaction())
			{
				IMultiCriteria mc = session.CreateMultiCriteria();
				mc = mc.Add(NHibernate.Criterion.DetachedCriteria.For(typeof(Product)).Add(NHibernate.Criterion.Restrictions.Lt(NHibernate.Criterion.Projections.Property("Price"), 10000)));
				mc = mc.Add(session.QueryOver<Customer>());
				mc = mc.Add(NHibernate.Criterion.DetachedCriteria.For(typeof(Order)).SetProjection(NHibernate.Criterion.Projections.Distinct(NHibernate.Criterion.Projections.Property("Date"))));

				IList results = mc.List();

				IEnumerable<Product> products = (results[0] as IList).OfType<Product>();
				IEnumerable<Customer> customers = (results[1] as IList).OfType<Customer>();
				IEnumerable<DateTime> dates = (results[2] as IList).OfType<DateTime>();
			}
		}

		static void LinqExtensions(ISessionFactory sessionFactory)
		{
			using (ISession session = sessionFactory.OpenSession())
			{
				String soundexName = session.Query<Customer>().Select(x => x.Name.Soundex()).First();
				IEnumerable<Product> products = session.Query<Product>().Where(x => SqlMethods.Like(x.Name, "%phone%")).ToList();
			}
		}

		static void HqlExtensions(ISessionFactory sessionFactory)
		{
			sessionFactory.RegisterFunction<DateTime>("last_week", "DATEADD(DAY, -7, GETDATE())");

			using (ISession session = sessionFactory.OpenSession())
			{
				IEnumerable<Order> recentOrders = session.CreateQuery("from Order o where o.Date >= last_week()").List<Order>();
			}
		}

		static void Polymorphism(ISessionFactory sessionFactory)
		{
			using (ISession session = sessionFactory.OpenSession())
			{
				IEnumerable<Person> allPeopleFromLinq = session.Query<Person>().ToList();
				IEnumerable<Person> allPeopleFromHql = session.CreateQuery("from Person").List<Person>();
				IEnumerable<Person> allPeopleFromCriteria = session.CreateCriteria(typeof(Person)).List<Person>();
				IEnumerable<Person> allPeopleFromQueryOver = session.QueryOver<Person>().List<Person>();
				IEnumerable<NationalCitizen> nationalCitizensFromLinq = session.Query<NationalCitizen>().ToList();
				IEnumerable<NationalCitizen> nationalCitizensFromLinq2 = session.Query<Person>().Where(x => x is NationalCitizen).Cast<NationalCitizen>().ToList();
				IEnumerable<ForeignCitizen> foreignCitizensFromLinq = session.Query<Person>().Where(x => x is ForeignCitizen).Cast<ForeignCitizen>().ToList();
				IEnumerable<NationalCitizen> nationalCitizenFromCriteria = session.CreateCriteria(typeof(Person), "p").Add(NHibernate.Criterion.Property.ForName("p.class").Eq(typeof(NationalCitizen))).List<NationalCitizen>();
				IEnumerable<NationalCitizen> nationalCitizenFromQueryOver = session.QueryOver<Person>().Where(x => x.GetType() == typeof(NationalCitizen)).List<NationalCitizen>();
				IEnumerable<NationalCitizen> nationalCitizensFromHql = session.CreateQuery("from Person p where p.class = Succintly.Model.NationalCitizen").List<NationalCitizen>();
				IEnumerable<ForeignCitizen> foreignCitizensFromHql = session.CreateQuery("from ForeignCitizen").List<ForeignCitizen>();
			}
		}

		static void UserTypes(ISessionFactory sessionFactory)
		{
			using (ISession session = sessionFactory.OpenSession())
			{
				var productWithPicture = session.Query<Product>().Where(x => x.Picture != null).Single();
			}
		}

		static void Locking(ISessionFactory sessionFactory)
		{
			using (ISession session = sessionFactory.OpenSession())
			using (session.BeginTransaction())
			{				
				Product p = session.Get<Product>(1, LockMode.Upgrade);

				LockMode mode = session.GetCurrentLockMode(p);

				session.Lock(p, LockMode.Force);

				mode = session.GetCurrentLockMode(p);

				session.Lock(p, LockMode.Upgrade);

				session.CreateQuery("from Product p").SetLockMode("p", LockMode.Upgrade);


				session.CreateCriteria(typeof(Product)).SetLockMode(LockMode.Upgrade).List<Product>();

				session.QueryOver<Product>().Lock().Upgrade.List();
			}
		}		

		static void Main(String[] args)
		{
			log4net.Config.XmlConfigurator.Configure();
			
			Configuration cfg = BuildLoquaciousExplicitConfiguration();

			//GenerateSchema(cfg);

			//Auditing(cfg);

			//Filters(cfg);

			//Validation(cfg);

			//NamedQueries(cfg);

			//Interceptor(cfg);

			//Loader(cfg);

			//Restrictions(cfg);

			//SqlOperations(cfg);

			//SoftDeletes(cfg);

			//Statistics(cfg);

			//Listeners(cfg);

			using (ISessionFactory sessionFactory = cfg.BuildSessionFactory())
			{
				//Add base items - only run this once
				//AddItems(sessionFactory);

				//Locking(sessionFactory);

				//Polymorphism(sessionFactory);

				//ExecutableHql(sessionFactory);

				//TwoSessions(sessionFactory);

				//QueryOverQuerying(sessionFactory);

				//SqlQuerying(sessionFactory);

				//CriteriaQuerying(sessionFactory);

				//HqlQuerying(sessionFactory);

				LinqQuerying(sessionFactory);

				//LinqQueryManipulation(sessionFactory);

				//Context(sessionFactory);

				//BulkInserts(sessionFactory);
				 
				//Streaming(sessionFactory);

				//DetachedQueries(sessionFactory);

				//CollectionFiltering(sessionFactory);

				//Futures(sessionFactory);

				//LinqExtensions(sessionFactory);

				//HqlExtensions(sessionFactory);

				//UserTypes(sessionFactory);

				//Transactions(sessionFactory);

				//MultiQueries(sessionFactory);				
			}
		}
	}
}
