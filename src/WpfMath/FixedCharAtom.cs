using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WpfMath
{
    // Atom representing character that does not depend on text style.
    internal class FixedCharAtom : CharSymbol
    {
        public FixedCharAtom(CharFont charFont)
        {
            this.CharFont = charFont;
        }

        public CharFont CharFont
        {
            get;
            private set;
        }

        public override Atom Copy()
        {
            return CopyTo(new FixedCharAtom(CharFont));
        }

        public override CharFont GetCharFont(ITeXFont texFont)
        {
            return this.CharFont;
        }

        protected override Box CreateBoxCore(TexEnvironment environment)
        {
            var charInfo = environment.MathFont.GetCharInfo(this.CharFont, environment.Style);
            return new CharBox(environment, charInfo);
        }
    }
}
