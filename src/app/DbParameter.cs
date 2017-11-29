using System.Data;

namespace Codentia.Common.Data
{
    /// <summary>
    /// DbParameter class
    /// </summary>
    public class DbParameter
    {
        private string _name;
        private DbType _dataType;
        private ParameterDirection _direction;
        private int _size;
        private byte _scale;
        private byte _precision;
        private object _value;
        private bool _isTableType;
        private string _tableTypeName;
        private DataTable _tableData;

        /// <summary>
        /// Initializes a new instance of the <see cref="DbParameter"/> class.
        /// </summary>
        /// <param name="parameterName">Name of the parameter.</param>
        /// <param name="dataType">Type of the data.</param>
        public DbParameter(string parameterName, DbType dataType)
            : this(parameterName, dataType, ParameterDirection.Input, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DbParameter"/> class.
        /// </summary>
        /// <param name="parameterName">Name of the parameter.</param>
        /// <param name="dataType">Type of the data.</param>
        /// <param name="value">The value.</param>
        public DbParameter(string parameterName, DbType dataType, object value)
            : this(parameterName, dataType, ParameterDirection.Input, value)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DbParameter"/> class.
        /// </summary>
        /// <param name="parameterName">Name of the parameter.</param>
        /// <param name="dataType">Type of the data.</param>
        /// <param name="direction">The direction.</param>
        /// <param name="value">The value.</param>
        public DbParameter(string parameterName, DbType dataType, ParameterDirection direction, object value)
            : this(parameterName, dataType, 0, direction, value)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DbParameter"/> class.
        /// </summary>
        /// <param name="parameterName">Name of the parameter.</param>
        /// <param name="dataType">Type of the data.</param>
        /// <param name="size">The size.</param>
        /// <param name="value">The value.</param>
        public DbParameter(string parameterName, DbType dataType, int size, object value)
            : this(parameterName, dataType, size, ParameterDirection.Input, value)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DbParameter"/> class.
        /// </summary>
        /// <param name="parameterName">Name of the parameter.</param>
        /// <param name="dataType">Type of the data.</param>
        /// <param name="size">The size.</param>
        /// <param name="direction">The direction.</param>
        /// <param name="value">The value.</param>
        public DbParameter(string parameterName, DbType dataType, int size, ParameterDirection direction, object value)
        {
            _name = parameterName;
            _dataType = dataType;
            _value = value;
            _direction = direction;
            _size = size;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DbParameter" /> class.
        /// </summary>
        /// <param name="parameterName">Name of the parameter.</param>
        /// <param name="tableTypeName">Name of the table type.</param>
        /// <param name="value">The value.</param>
        public DbParameter(string parameterName, string tableTypeName, DataTable value)
        {
            _name = parameterName;
            _dataType = DbType.Object;
            _isTableType = true;
            _direction = ParameterDirection.Input;
            _tableTypeName = tableTypeName;
            _tableData = value;
        }

        /// <summary>
        /// Gets or sets the type of the db.
        /// </summary>
        /// <value>The type of the db.</value>
        public DbType DbType
        {
            get
            {
                return _dataType;
            }

            set
            {
                _dataType = value;
            }
        }

        /// <summary>
        /// Gets or sets the direction.
        /// </summary>
        /// <value>The direction.</value>
        public ParameterDirection Direction
        {
            get
            {
                return _direction;
            }

            set
            {
                _direction = value;
            }
        }

        /// <summary>
        /// Gets or sets the name of the parameter.
        /// </summary>
        /// <value>The name of the parameter.</value>
        public string ParameterName
        {
            get
            {
                return _name;
            }

            set
            {
                _name = value;
            }
        }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>The value.</value>
        public object Value
        {
            get
            {
                return _value;
            }

            set
            {
                _value = value;
            }
        }

        /// <summary>
        /// Gets or sets the size.
        /// </summary>
        /// <value>The size.</value>
        public int Size
        {
            get
            {
                return _size;
            }

            set
            {
                _size = value;
            }
        }

        /// <summary>
        /// Gets or sets the scale.
        /// </summary>
        /// <value>The scale.</value>
        public byte Scale
        {
            get
            {
                return _scale;
            }

            set
            {
                _scale = value;
            }
        }

        /// <summary>
        /// Gets or sets the precision.
        /// </summary>
        /// <value>The precision.</value>
        public byte Precision
        {
            get
            {
                return _precision;
            }

            set
            {
                _precision = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is table type.
        /// </summary>
        public bool IsTableType
        {
            get
            {
                return _isTableType;
            }

            set
            {
                _isTableType = value;
            }
        }

        /// <summary>
        /// Gets or sets the name of the table type.
        /// </summary>
        public string TableTypeName
        {
            get
            {
                return _tableTypeName;
            }

            set
            {
                _tableTypeName = value;
            }
        }

        /// <summary>
        /// Gets or sets the table data.
        /// </summary>
        public DataTable TableData
        {
            get
            {
                return _tableData;
            }

            set
            {
                _tableData = value;
            }
        }
    }
}
