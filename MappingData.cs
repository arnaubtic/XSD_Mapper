using System.Collections.Generic;

namespace XSD_Mapper
{
    public class MappingData
    {
        public string? ConnectionString { get; set; }
        public List<Mapping>? Mappings { get; set; }
    }

    public class Mapping
    {
        public string? TableName { get; set; }
        public List<Column>? Columns { get; set; }
    }

    public class Column
    {
        public string? ColumnName { get; set; }
        public string? XmlNodePath { get; set; }
        public Column? Parent { get; set; }
    }
}
