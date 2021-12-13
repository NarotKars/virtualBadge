using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccess.Exceptions
{
    public abstract class Exceptions : Exception
    {
        public Exceptions(string message) : base(message) { }
    }

    public class KeyCloakException : Exceptions
    {
        public int statusCode;

        public KeyCloakException(string message, int statusCode) : base(message) {
            this.statusCode = statusCode;
        }
    }
}
