using System;
using System.Linq;
using System.Collections;
using System.Reflection;
using NHibernate.Cfg;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Mapping.ByCode;
using System.Text;
using NHibernate.Type;
using NHibernate.Mapping;
using System.Collections.Generic;

namespace Succinctly.Common
{
	public sealed class ConventionMapping : BaseMapping
	{
		public ConventionMapping(Assembly assembly) : this(assembly.FullName)
		{
		}

		public ConventionMapping(Type type) : this(type.Assembly)
		{

		}

		public ConventionMapping(String assemblyName)
		{
			this.EntitiesAssemblyName = assemblyName;
		}

		public String EntitiesAssemblyName
		{
			get;
			private set;
		}

		public override Configuration Map(Configuration cfg)
		{
			ConventionModelMapper mapper = new ConventionModelMapper();
			mapper.IsEntity((x, y) => this.IsEntity(x, y, this.EntitiesAssemblyName));
			mapper.IsRootEntity((x, y) => this.IsRootEntity(x, y, this.EntitiesAssemblyName));
			mapper.IsOneToMany((x, y) => this.IsOneToMany(x, y));
			mapper.IsManyToOne((x, y) => this.IsManyToOne(x, y));
			mapper.IsManyToMany((x, y) => this.IsManyToMany(x, y));
			mapper.IsBag((x, y) => this.IsBag(x, y));
			mapper.IsSet((x, y) => this.IsSet(x, y));
			mapper.IsProperty((x, y) => this.IsProperty(x, y));
			mapper.IsPersistentProperty((x, y) => this.IsPersistentProperty(x, y));
			mapper.BeforeMapClass += this.BeforeMapClass;
			mapper.BeforeMapProperty += this.BeforeMapProperty;
			mapper.BeforeMapSet += this.BeforeMapSet;
			mapper.BeforeMapOneToMany += this.BeforeMapOneToMany;
			mapper.BeforeMapManyToOne += this.BeforeMapManyToOne;
			mapper.BeforeMapManyToMany += BeforeMapManyToMany;

			HbmMapping mappings = mapper.CompileMappingFor(Assembly.Load(this.EntitiesAssemblyName).GetExportedTypes());

			cfg.AddMapping(mappings);

			return (cfg);
		}

		protected void BeforeMapManyToMany(IModelInspector modelInspector, PropertyPath member, IManyToManyMapper collectionRelationManyToManyCustomizer)
		{
			collectionRelationManyToManyCustomizer.Lazy(LazyRelation.Proxy);
			Type destinationType = member.LocalMember.GetPropertyOrFieldType().GetGenericArguments()[0];
			collectionRelationManyToManyCustomizer.Column(this.GetNormalizedDbName(String.Concat(destinationType.Name, "ID")));
			collectionRelationManyToManyCustomizer.Class(destinationType);
		}

		protected void BeforeMapManyToOne(IModelInspector modelInspector, PropertyPath member, IManyToOneMapper propertyCustomizer)
		{
			propertyCustomizer.Column(this.GetNormalizedDbName(String.Concat(member.LocalMember.GetPropertyOrFieldType().Name, "ID")));
			propertyCustomizer.Cascade(Cascade.Detach);
			propertyCustomizer.Lazy(LazyRelation.NoProxy);
			propertyCustomizer.Fetch(FetchKind.Select);
		}

		protected void BeforeMapOneToMany(IModelInspector modelInspector, PropertyPath member, IOneToManyMapper collectionRelationOneToManyCustomizer)
		{
			collectionRelationOneToManyCustomizer.Class(member.LocalMember.GetPropertyOrFieldType().GetGenericArguments()[0]);
		}

		protected void BeforeMapSet(IModelInspector modelInspector, PropertyPath member, ISetPropertiesMapper propertyCustomizer)
		{
			propertyCustomizer.Cascade(Cascade.All | Cascade.DeleteOrphans);
			propertyCustomizer.Lazy(CollectionLazy.Lazy);
			propertyCustomizer.Fetch(CollectionFetchMode.Select);
			propertyCustomizer.BatchSize(10);
			
			Type collectionType = member.LocalMember.GetPropertyOrFieldType();

			if (collectionType.IsGenericCollection())
			{
				Type entityType = collectionType.GetGenericArguments()[0];

				if ((entityType == typeof(String)) || (entityType.IsPrimitive == true) || (entityType.IsEnum == true) || (entityType == typeof(DateTime)))
				{
					//collection of elements
				}
				else
				{
					propertyCustomizer.Inverse(true);
				}
			}
		}

		protected void BeforeMapProperty(IModelInspector modelInspector, PropertyPath member, IPropertyMapper propertyCustomizer)
		{
			propertyCustomizer.Column(this.GetNormalizedDbName(member.LocalMember.Name));

			if (member.LocalMember.GetPropertyOrFieldType().IsEnum == true)
			{
				//propertyCustomizer.Type(Activator.CreateInstance(typeof(EnumStringType<>).MakeGenericType(member.LocalMember.GetPropertyOrFieldType())) as IType);
				propertyCustomizer.Type(Activator.CreateInstance(typeof(EnumType<>).MakeGenericType(member.LocalMember.GetPropertyOrFieldType())) as IType);
			}
		}

		protected void BeforeMapClass(IModelInspector modelInspector, Type type, IClassAttributesMapper classCustomizer)
		{
			classCustomizer.Lazy(true);
			classCustomizer.Table(this.GetNormalizedDbName(type.Name));
			classCustomizer.Id(type.GetProperty(String.Concat(type.Name, "Id")), map =>
			{
				map.Column(this.GetNormalizedDbName(String.Concat(type.Name, "ID")));
				map.Generator(new HighLowGeneratorDef());
			});
		}

		protected Boolean IsBag(MemberInfo x, Boolean y)
		{
			return (false);
		}

		protected Boolean IsManyToMany(MemberInfo x, Boolean y)
		{
			return (true);
		}

		protected Boolean IsManyToOne(MemberInfo x, Boolean y)
		{
			Boolean isManyToOne = (typeof(IEnumerable).IsAssignableFrom(x.GetPropertyOrFieldType()) == false && x.GetPropertyOrFieldType().IsEnum == false && x.GetPropertyOrFieldType().IsClass == true);

			return (isManyToOne);
		}

		protected Boolean IsOneToMany(MemberInfo member, Boolean y)
		{
			Type collectionType = member.GetPropertyOrFieldType().GetGenericTypeDefinition();
			Type sourceEntity = member.DeclaringType;
			Type destinationEntity = member.GetPropertyOrFieldType().GetGenericArguments()[0];

			if (this.IsEntity(destinationEntity, true, this.EntitiesAssemblyName) == false)
			{
				return (false);
			}

			if (typeof(ICollection<>).MakeGenericType(destinationEntity).IsAssignableFrom(member.GetPropertyOrFieldType()) == true)
			{
				if (destinationEntity.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetProperty).Where(x => x.PropertyType.IsGenericCollection() == true && x.PropertyType == member.GetPropertyOrFieldType().GetGenericTypeDefinition().MakeGenericType(sourceEntity)).Any() == true)
				{
					return (false);
				}
			}

			return (true);
		}

		protected Boolean IsSet(MemberInfo x, Boolean y)
		{
			Boolean isSet = (typeof(IEnumerable).IsAssignableFrom(x.GetPropertyOrFieldType())) && (x.GetPropertyOrFieldType() != typeof(String));

			return (isSet);
		}

		protected Boolean IsRootEntity(Type x, Boolean y, String assemblyName)
		{
			Boolean isRootEntity = (x.IsEnum == false && x.IsInterface == false && x.Assembly.FullName == assemblyName);

			return (isRootEntity);
		}

		protected Boolean IsEntity(Type x, Boolean y, String assemblyName)
		{
			Boolean isRootEntity = (x.IsEnum == false && x.IsInterface == false && x.IsAbstract == false && x.Assembly.FullName == assemblyName);

			return (isRootEntity);
		}

		protected Boolean IsPersistentProperty(MemberInfo x, Boolean y)
		{
			return ((x is PropertyInfo) && ((x as PropertyInfo).CanWrite == true) && ((x as PropertyInfo).GetSetMethod() != null));
		}

		protected Boolean IsProperty(MemberInfo x, Boolean y)
		{
			Boolean isProperty = ((x is PropertyInfo) && ((x as PropertyInfo).CanWrite == true) && ((x as PropertyInfo).GetSetMethod() != null) && (x.GetPropertyOrFieldType().IsGenericType == false) && (x.GetPropertyOrFieldType() == typeof(String) || x.GetPropertyOrFieldType().IsPrimitive == true || x.GetPropertyOrFieldType().IsEnum == true || Nullable.GetUnderlyingType(x.GetPropertyOrFieldType()) != null || x.GetPropertyOrFieldType() == typeof(Byte[]) || x.GetPropertyOrFieldType() == typeof(DateTime)));

			return (isProperty);
		}		
	}
}
