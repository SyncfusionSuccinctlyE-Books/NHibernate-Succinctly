using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using NHibernate;
using NHibernate.SqlTypes;
using NHibernate.UserTypes;
using System.Reflection;

namespace Succinctly.Common
{
	[Serializable]
	public sealed class ImageUserType : IUserType, IParameterizedType
	{
		private Byte[] data = null;

		public ImageUserType() : this(ImageFormat.Png)
		{
		}

		public ImageUserType(ImageFormat imageFormat)
		{
			this.ImageFormat = imageFormat;
		}

		public ImageFormat ImageFormat
		{
			get;
			private set;
		}

		public override Int32 GetHashCode()
		{
			return ((this as IUserType).GetHashCode(this.data));
		}

		public override Boolean Equals(Object obj)
		{
			ImageUserType other = obj as ImageUserType;

			if (other == null)
			{
				return (false);
			}

			if (Object.ReferenceEquals(this, other) == true)
			{
				return (true);
			}

			return (this.data.SequenceEqual(other.data));
		}

		Boolean IUserType.IsMutable
		{
			get
			{
				return (true);
			}
		}

		Object IUserType.Assemble(Object cached, Object owner)
		{
			return (cached);
		}

		Object IUserType.DeepCopy(Object value)
		{
			if (value is ICloneable)
			{
				return ((value as ICloneable).Clone());
			}
			else
			{
				return (value);
			}
		}

		Object IUserType.Disassemble(Object value)
		{
			return ((this as IUserType).DeepCopy(value));
		}

		Boolean IUserType.Equals(Object x, Object y)
		{
			return (Object.Equals(x, y));
		}

		Int32 IUserType.GetHashCode(Object x)
		{
			return ((x != null) ? x.GetHashCode() : 0);
		}

		Object IUserType.NullSafeGet(IDataReader rs, String[] names, Object owner)
		{
			this.data = NHibernateUtil.Binary.NullSafeGet(rs, names) as Byte[];

			if (data == null)
			{
				return (null);
			}

			using (Stream stream = new MemoryStream(this.data ?? new Byte[0]))
			{
				return (Image.FromStream(stream));
			}
		}

		void IUserType.NullSafeSet(IDbCommand cmd, Object value, Int32 index)
		{
			if (value != null)
			{
				Image data = value as Image;

				using (MemoryStream stream = new MemoryStream())
				{
					data.Save(stream, this.ImageFormat);
					value = stream.ToArray();
				}
			}

			NHibernateUtil.Binary.NullSafeSet(cmd, value, index);
		}

		Object IUserType.Replace(Object original, Object target, Object owner)
		{
			return (original);
		}

		Type IUserType.ReturnedType
		{
			get
			{
				return (typeof(Image));
			}
		}

		SqlType[] IUserType.SqlTypes
		{
			get
			{
				return (new SqlType[] { NHibernateUtil.BinaryBlob.SqlType });
			}
		}

		#region IParameterizedType Members

		void IParameterizedType.SetParameterValues(IDictionary<String, String> parameters)
		{
			if ((parameters != null) && (parameters.ContainsKey("ImageFormat") == true))
			{
				this.ImageFormat = typeof(ImageFormat).GetProperty(parameters["ImageFormat"], BindingFlags.Static | BindingFlags.Public | BindingFlags.GetProperty).GetValue(null, null) as ImageFormat;
			}
		}

		#endregion
	}
}