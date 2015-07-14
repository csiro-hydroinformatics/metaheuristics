using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSIRO.Utilities
{
    public class ReportTableBuilder
    {
        public ReportTableBuilder( ) { }

        protected void addRow( IDictionary<string, string> newRow )
        {
            foreach( string key in newRow.Keys )
            {
                if( !this.columnNames.Contains( key.ToString( ) ) )
                    this.columnNames.Add( key.ToString( ) );
            }
            this.Table.Add( new Dictionary<string, string>( newRow ) );
        }

        private List<string> columnNames = new List<string>( );

        protected ICollection<string> ColumnNames
        {
            get { return columnNames.AsReadOnly( ); }
        }

        public void SortColumnNames( IComparer<string> comparer )
        {
            this.columnNames.Sort( comparer );
        }

        public void SetColumnNames( string[] columnNames )
        {
            this.columnNames = new List<string>( columnNames );
        }

        /// <summary>
        /// maps rows then to columns.
        /// </summary>
        protected List<Dictionary<string, string>> Table = new List<Dictionary<string, string>>( );

        public string GetReportTable( )
        {
            return getReportTable( new List<string>(columnNames) );
        }

        private string getReportTable( List<string> columnNames )
        {
            return DelimiterSeparatedValues.SerialiseAsCommaSeparated( getReportTableInArray( columnNames ) );
        }

        protected string[][] getReportTableInArray( ICollection<string> columnNames )
        {
            string[][] result = new string[Table.Count + 1][];
            // Add header
            List<string> newRow = new List<string>( );
            foreach( string key in columnNames )
                newRow.Add( key );
            result[0] = newRow.ToArray( );
            for( int entry = 0; entry < Table.Count; entry++ )
            {
                newRow = new List<string>( );
                Dictionary<string, string> row = Table[entry];
                foreach( string key in columnNames )
                {
                    newRow.Add( row.ContainsKey( key ) ? row[key] : string.Empty );
                }
                result[entry + 1]/*because of header offset*/ = newRow.ToArray( );
            }
            return result;
        }

        public void AddRow( IDataFrameInfoProvider iDataFrameInfoProvider )
        {
            this.addRow( iDataFrameInfoProvider.GetRow( ) );
        }
    }
}
