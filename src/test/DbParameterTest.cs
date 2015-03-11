using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using NUnit.Framework;

namespace Codentia.Common.Data.Test
{
    /// <summary>
    /// Unit testing framework for DbParameter class
    /// </summary>
    [TestFixture]
    public class DbParameterTest
    {
        /// <summary>
        /// Scenario: Object constructed, properties tested
        /// Expected: Input values for those set, anticipated default values for others
        /// </summary>
        [Test]
        public void _001_Constructor_NameTypeValue()
        {
            // construct object
            DbParameter param = new DbParameter("param1", DbType.Boolean, true);

            // test all properties            
            Assert.That(param.ParameterName, Is.EqualTo("param1"));            
            Assert.That(param.DbType, Is.EqualTo(DbType.Boolean));
            Assert.That(param.Direction, Is.EqualTo(ParameterDirection.Input));
            Assert.That(param.Value, Is.True);      
            Assert.That(param.Size, Is.EqualTo(0));
            Assert.That(param.Scale, Is.EqualTo(0));
            Assert.That(param.Precision, Is.EqualTo(0));
        }

        /// <summary>
        /// Scenario: Object constructed, properties tested
        /// Expected: Input values for those set, anticipated default values for others
        /// </summary>
        [Test]
        public void _002_Constructor_NameTypeValueDirection()
        {
            // construct object
            DbParameter param = new DbParameter("param2", DbType.Int32, ParameterDirection.Output, 17);

            // test all properties
            Assert.That(param.ParameterName, Is.EqualTo("param2"));
            Assert.That(param.DbType, Is.EqualTo(DbType.Int32));
            Assert.That(param.Direction, Is.EqualTo(ParameterDirection.Output));
            Assert.That(param.Value, Is.EqualTo(17));                       
            Assert.That(param.Size, Is.EqualTo(0));
            Assert.That(param.Scale, Is.EqualTo(0));
            Assert.That(param.Precision, Is.EqualTo(0));
        }

        /// <summary>
        /// Scenario: Object constructed, properties tested
        /// Expected: Input values for those set, anticipated default values for others
        /// </summary>
        [Test]
        public void _003_Constructor_NameTypeValueDirectionSize()
        {
            // construct object
            DbParameter param = new DbParameter("param2", DbType.StringFixedLength, 10, ParameterDirection.InputOutput, "weeble");

            // test all properties
            Assert.That(param.ParameterName, Is.EqualTo("param2"));
            Assert.That(param.DbType, Is.EqualTo(DbType.StringFixedLength));
            Assert.That(param.Direction, Is.EqualTo(ParameterDirection.InputOutput));
            Assert.That(param.Value, Is.EqualTo("weeble"));
            Assert.That(param.Size, Is.EqualTo(10));
            Assert.That(param.Scale, Is.EqualTo(0));
            Assert.That(param.Precision, Is.EqualTo(0)); 
        }

        /// <summary>
        /// Scenario: Object constructed, properties tested
        /// Expected: Input values for those set, anticipated default values for others
        /// </summary>
        [Test]
        public void _004_Constructor_NameTypeValueSize()
        {
            // construct object
            DbParameter param = new DbParameter("param2", DbType.StringFixedLength, 10, "weeble");

            // test all properties
            Assert.That(param.ParameterName, Is.EqualTo("param2"));
            Assert.That(param.DbType, Is.EqualTo(DbType.StringFixedLength));
            Assert.That(param.Direction, Is.EqualTo(ParameterDirection.Input));
            Assert.That(param.Value, Is.EqualTo("weeble"));
            Assert.That(param.Size, Is.EqualTo(10));
            Assert.That(param.Scale, Is.EqualTo(0));
            Assert.That(param.Precision, Is.EqualTo(0));
        }

        /// <summary>
        /// Scenario: Object constructed, properties tested
        /// Expected: Input values for those set, anticipated default values for others
        /// </summary>
        [Test]
        public void _004b_Constructor_NameType()
        {
            // construct object
            DbParameter param = new DbParameter("param2", DbType.StringFixedLength);

            // test all properties
            Assert.That(param.ParameterName, Is.EqualTo("param2"));
            Assert.That(param.DbType, Is.EqualTo(DbType.StringFixedLength));
            Assert.That(param.Direction, Is.EqualTo(ParameterDirection.Input));
            Assert.That(param.Value, Is.Null);
            Assert.That(param.Size, Is.EqualTo(0));
            Assert.That(param.Scale, Is.EqualTo(0));
            Assert.That(param.Precision, Is.EqualTo(0));
        }

        /// <summary>
        /// Scenario: Get/Set property
        /// Expected: Value setted can be 'getted'
        /// </summary>
        [Test]
        public void _005_DbType_GetSet()
        {
            DbParameter param = new DbParameter("param1", DbType.Boolean, true);
            param.DbType = DbType.Xml;
            Assert.That(param.DbType, Is.EqualTo(DbType.Xml));
        }

        /// <summary>
        /// Scenario: Get/Set property
        /// Expected: Value setted can be 'getted'
        /// </summary>
        [Test]
        public void _006_Direction_GetSet()
        {
            DbParameter param = new DbParameter("param1", DbType.Boolean, true);
            param.Direction = ParameterDirection.Output;
            Assert.That(param.Direction, Is.EqualTo(ParameterDirection.Output));
        }

        /// <summary>
        /// Scenario: Get/Set property
        /// Expected: Value setted can be 'getted'
        /// </summary>
        [Test]
        public void _007_ParameterName_GetSet()
        {
            DbParameter param = new DbParameter("param1", DbType.Boolean, true);
            param.ParameterName = "param2";            
            Assert.That(param.ParameterName, Is.EqualTo("param2"));
        }

        /// <summary>
        /// Scenario: Get/Set property
        /// Expected: Value setted can be 'getted'
        /// </summary>
        [Test]
        public void _008_Value_GetSet()
        {
            DbParameter param = new DbParameter("param1", DbType.Boolean, true);
            param.Value = false;
            Assert.That(param.Value, Is.False);
        }

        /// <summary>
        /// Scenario: Get/Set property
        /// Expected: Value setted can be 'getted'
        /// </summary>
        [Test]
        public void _009_Size_GetSet()
        {
            DbParameter param = new DbParameter("param1", DbType.Boolean, true);
            param.Size = 25;
            Assert.That(param.Size, Is.EqualTo(25));
        }

        /// <summary>
        /// Scenario: Get/Set property
        /// Expected: Value setted can be 'getted'
        /// </summary>
        [Test]
        public void _010_Scale_GetSet()
        {
            DbParameter param = new DbParameter("param1", DbType.Boolean, true);
            param.Scale = 5;            
            Assert.That(param.Scale, Is.EqualTo(5));
        }

        /// <summary>
        /// Scenario: Get/Set property
        /// Expected: Value setted can be 'getted'
        /// </summary>
        [Test]
        public void _011_Precision_GetSet()
        {
            DbParameter param = new DbParameter("param1", DbType.Boolean, true);
            param.Precision = 10;            
            Assert.That(param.Precision, Is.EqualTo(10));
        }
    }
}
