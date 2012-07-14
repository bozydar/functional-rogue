using System.Collections.Generic;
using FarseerPhysics.Dynamics;

namespace Ruminate.Utils {

    public static class FarseerExtentions {

        public static HashSet<Body> GetTouching(this Body body) {

            var bodies = new HashSet<Body>();
            var contact = body.ContactList;

            while (contact != null) {
                if (contact.Contact.IsTouching() && 
                    contact.Other.BodyType != BodyType.Dynamic) {
                    bodies.Add(contact.Other);
                }
                contact = contact.Next;
            }

            return bodies;
        }

        public static bool IsTouching(this Body body) {

            var contact = body.ContactList;

            while (contact != null) {
                if (contact.Contact.IsTouching()) { return true; }
                contact = contact.Next;
            } 

            return false;
        }
    }
}
