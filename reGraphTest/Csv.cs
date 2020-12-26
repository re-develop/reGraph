using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TMYConverter
{
    public class Csv
    {
        private List<string> _columns = new List<string>();
        private List<List<object>> _data = new List<List<object>>();
        private char _seperator = ',';
        private Dictionary<string, Func<string, object>> _converters;
        private Func<string, bool> _filter;

        public int Count => _data.Count;

        public Csv(string file, bool readColumns = true, char seperator = ',', int linesToSkip = 0, Dictionary<string, Func<string, object>> converters = null, Func<string, bool> filter = null)
        {
            // save inputted data to class
            _seperator = seperator;
            _converters = converters ?? new Dictionary<string, Func<string, object>>();
            _filter = filter ?? new Func<string, bool>(x => true);

            var content = File.ReadAllLines(file); // get all lines from input file
            var iter = content.Skip(linesToSkip); // skip specified amount of lines


            if (readColumns) // should parse column?
            {
                parseColumns(iter.Take(1).First());
                iter = iter.Skip(1);
            }

            parseData(iter); // parse data in csv
        }

        public object GetData(int row, int column) // get data at specific row/column returns null if out of bounds
        {
            if (column < 0 || column >= _columns.Count || row < 0 || row >= Count)
                return null;

            return _data[row][column];
        }

        public object GetData(int row, string column) // get data at specific row/column returns null if out of bounds
        {
            var columnIndex = GetColumnIndex(column);
            if (columnIndex == -1 || row < 0 || row >= Count)
                return null;

            return _data[row][columnIndex];
        }

        public List<object> GetData(string column) // get data at specific row returns null if out of bounds
        {
            var columnIndex = GetColumnIndex(column);
            if (columnIndex == -1)
                return null;

            return _data.Select(x => x[columnIndex]).ToList();

        }

        public List<object> GetData(int row) // get data at specific row returns null if out of bounds
        {
            if (row < 0 || row >= Count)
                return null;

            return _data[row];
        }

        public int GetColumnIndex(string fieldName) // get column index by name returns -1 if not found
        {
            return _columns.IndexOf(fieldName);
        }

        public string GetColumnName(int index) // get name for column index returns null if out of bounds
        {
            if (index < 0 || index >= _columns.Count)
                return null;

            return _columns[index];
        }

        public IEnumerable<string> GetColumns() // returns all column names
        {
            return _columns;
        }

        private void parseColumns(string line) // parse columns from csv header
        {
            _columns = line.Split(_seperator).ToList();
        }

        private void parseData(IEnumerable<string> lines) // parse data from csv body
        {
            foreach (var line in lines) // iterate over all lines
            {
                var data = line.Split(_seperator); // split data by seperator
                List<object> res = new List<object>(); // result set
                for (int i = 0; i < data.Length; i++) // irate over all columns
                {
                    var name = GetColumnName(i); // get column name
                    if (_filter(name) == false) // check if column should be ignored
                        continue;

                    if (_converters.ContainsKey(name)) // check if converter exists for column
                        res.Add(_converters[name](data[i])); // apply converter and save to result set
                    else
                        res.Add(data[i]); // save to result set
                }

                _data.Add(res); // add result set to internal data
            }

            _columns = _columns.Where(_filter).ToList(); // remove all ignored columns
        }

    }
}
