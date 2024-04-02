using Core.Objects.Extensions;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace Core.Data
{
    internal class ObjectDataReader<T> : IDataReader
    {
        public object this[int i] => GetValue(i);

        public object this[string name] => GetValue(GetOrdinal(name));

        public int Depth => 0;

        public bool IsClosed { get; private set; }

        public int RecordsAffected => 0;

        public int FieldCount => fields.Count;

        private readonly IDictionary<string, PropertyInfo> fields;
        private IEnumerator<T> source;
        private static readonly Type stringType = typeof(string);

        public ObjectDataReader(IEnumerable<T> items)
        {
            source = items.GetEnumerator();

            fields = typeof(T).GetProperties()
                .Where(p => p.PropertyType.IsValueType || p.PropertyType == stringType)
                .ToDictionary(p => p.Name, p => p, StringComparer.OrdinalIgnoreCase);
        }

        public void Close() => IsClosed = true;

        public bool GetBoolean(int i) => (bool)fields.ElementAt(i).Value.GetValue(source.Current);

        public byte GetByte(int i) => (byte)fields.ElementAt(i).Value.GetValue(source.Current);

        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length) => throw new NotImplementedException();

        public char GetChar(int i) => (char)fields.ElementAt(i).Value.GetValue(source.Current);

        public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length) => throw new NotImplementedException();

        public IDataReader GetData(int i) => null;

        public string GetDataTypeName(int i) => fields.ElementAt(i).Value.PropertyType.FullName;

        public DateTime GetDateTime(int i) => (DateTime)GetValue(i);

        public decimal GetDecimal(int i) => (decimal)GetValue(i);

        public double GetDouble(int i) => (double)GetValue(i);

        public Type GetFieldType(int i) => fields.ElementAt(i).Value.PropertyType;

        public float GetFloat(int i) => (float)GetValue(i);

        public Guid GetGuid(int i) => (Guid)GetValue(i);

        public short GetInt16(int i) => (short)GetValue(i);

        public int GetInt32(int i) => (int)GetValue(i);

        public long GetInt64(int i) => (long)GetValue(i);

        public string GetName(int i) => fields.ElementAt(i).Value.Name;

        public int GetOrdinal(string name) => fields.Keys.Select((k, i) => new { k, i }).First(a => a.k == name).i;

        public DataTable GetSchemaTable()
        {
            DataTable result = new();
            fields.ForEach(f => result.Columns.Add(f.Key, f.Value.PropertyType));
            return result;
        }

        public SqlBulkCopyColumnMapping[] GetMappings() => fields
                .Select(f => new SqlBulkCopyColumnMapping(f.Key, f.Key))
                .ToArray();

        public string GetString(int i) => (string)GetValue(i);

        public object GetValue(int i) => fields.ElementAt(i).Value.GetValue(source.Current);

        public int GetValues(object[] values)
        {
            int fieldCount = Math.Min(values.Length, fields.Keys.Count);

            for (int i = 0; i < fieldCount; i++)
                values[i] = GetValue(i);

            return fieldCount;
        }

        public bool IsDBNull(int i) => GetValue(i) == DBNull.Value;

        public bool NextResult() => throw new NotImplementedException();

        public bool Read() => source.MoveNext();

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool isDisposing)
        {
            if (isDisposing && source != null)
            {
                source.Dispose();
                source = null;
            }
        }
    }
}