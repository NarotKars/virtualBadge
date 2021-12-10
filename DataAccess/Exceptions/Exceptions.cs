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
        public KeyCloakException(string message) : base(message) { }
    }
}
