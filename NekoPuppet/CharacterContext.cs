using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoPuppet
{
    public class CharacterContext
    {
        private static CharacterContext singleton;
        private MainForm formContext;

        private CharacterContext(MainForm formContext)
        {
            this.formContext = formContext;
        }

        // available to any program referencing application
        public static CharacterContext GetCharacterContext()
        {
            if (singleton != null) return singleton;
            throw new Exception("Character Context not instantiated by main application");
        }

        // only available to main application
        internal static CharacterContext CreateCharacterContext(MainForm formContext)
        {
            if (singleton != null) return singleton;
            singleton = new CharacterContext(formContext);
            return singleton;
        }

        public CharacterControlInterface GetCharacter(int index)
        {
            switch (index)
            {
                case 0:
                    return formContext.CharacterInterface;
                case 1:
                    return formContext.CharacterOffhandInterface;

            }
            return null;
        }
    }
}
