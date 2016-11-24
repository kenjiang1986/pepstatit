using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Job.DTO
{
    /// <summary>
    /// 参数
    /// </summary>
    /// <remarks>
    ///     <para>    Creator：helang</para>
    ///     <para>CreatedTime：2013/7/18 12:44:36</para>
    /// </remarks>
    public class ParamesDTO
    {
        /// <summary>
        /// name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// value
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// size
        /// </summary>
        public int Size { set; get; }

        /// <summary>
        /// Direction
        /// </summary>
        public ParameterDirection Direction { get; set; }
    }
}
