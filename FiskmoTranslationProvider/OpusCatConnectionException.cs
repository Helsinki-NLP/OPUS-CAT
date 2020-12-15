using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FiskmoTranslationProvider
{
    public class OpusCatEngineConnectionException : Exception
    {
        public OpusCatEngineConnectionException()
        {
        }

        public OpusCatEngineConnectionException(string message) : base(message)
        {
        }

        public OpusCatEngineConnectionException(string message, Exception inner) : base(message,inner)
        {
        }
    }
}
