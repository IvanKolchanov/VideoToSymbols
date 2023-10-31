namespace VideoToSymbols
{
    class GradientProvider
    {
        public const int CONVERSION_4_SYM = 0, CONVERSION_8_SYM = 1, CONVERSION_16_SYM = 2;

        private static Dictionary<int, char> lightnessToSymbol4 = new Dictionary<int, char>();
        private static Dictionary<int, char> lightnessToSymbol8 = new Dictionary<int, char>();
        private static Dictionary<int, char> lightnessToSymbol16 = new Dictionary<int, char>();
        private static int[] lightnessList4;
        private static int[] lightnessList8;
        private static int[] lightnessList16;
        private static string symbolList4, symbolList8, symbolList16;

        static GradientProvider() {
            lightnessList4 = new int[] { 0, 250000, 500000, 750000 };
            symbolList4 = " 7@█";
            for (int i = 0; i < 4; i++) { lightnessToSymbol4[lightnessList4[i]] = symbolList4[i]; }

            lightnessList8 = new int[] { 0, 95119, 235990, 365309, 496444, 624157, 750000, 875000 };
            symbolList8 = " `;sZg@█";
            for (int i = 0; i < 8; i++) { lightnessToSymbol8[lightnessList8[i]] = symbolList8[i]; }

            lightnessList16 = new int[] { 0, 110973, 163658, 197237, 238527, 293830, 347204, 409947, 475071, 528232, 587630, 653068, 717357, 765425, 875000, 937500};
            symbolList16 = " `-:~=?vC2EH&N@█";
            for (int i = 0; i < 16; i++) { lightnessToSymbol16[lightnessList16[i]] = symbolList16[i]; }
        }

        public static string getSymbolList(int conversionRate)
        {
            switch (conversionRate)
            {
                case CONVERSION_4_SYM:
                    return symbolList4;
                case CONVERSION_8_SYM:
                    return symbolList8;
                case CONVERSION_16_SYM: 
                    return symbolList16;
                default: 
                    return null;
            }
        }

        public static Dictionary<int, char> getLightnessToSymbol(int conversionRate)
        {
            switch (conversionRate)
            {
                case CONVERSION_4_SYM:
                    return lightnessToSymbol4;
                case CONVERSION_8_SYM:
                    return lightnessToSymbol8;
                case CONVERSION_16_SYM:
                    return lightnessToSymbol16;
                default:
                    return null;
            }
        }

        public static int[] getLightnessList(int conversionRate)
        {
            switch (conversionRate)
            {
                case CONVERSION_4_SYM:
                    return lightnessList4;
                case CONVERSION_8_SYM:
                    return lightnessList8;
                case CONVERSION_16_SYM:
                    return lightnessList16;
                default:
                    return null;
            }
        }

        public static int getSymbolCount(int conversionRate)
        {
            switch (conversionRate)
            {
                case CONVERSION_16_SYM: return 2;
                case CONVERSION_8_SYM: return 8;
                case CONVERSION_4_SYM: return 4;
                default: return -1;
            }
        }

        public static byte[] getBytesFromSymbols(int conversionRate, int[] symbols)
        {
            if (conversionRate == CONVERSION_16_SYM)
            {
                byte result = (byte)(symbols[0] << 4);
                result |= (byte)symbols[1];
                return new byte[1] { result };
            }
            if (conversionRate == CONVERSION_8_SYM)
            {
                byte[] result = new byte[3];
                int symbolsToInt = 0;
                for (int i = 0; i < symbols.Length; i++)
                {
                    symbolsToInt <<= 3;
                    symbolsToInt |= symbols[i];
                }
                result[0] = (byte)(symbolsToInt >> 16);
                result[1] = (byte)((symbolsToInt >> 8) & 255);
                result[2] = (byte)(symbolsToInt & 255);
                return result;
            }
            if (conversionRate == CONVERSION_4_SYM)
            {
                byte result = (byte)(symbols[0] << 6);
                result |= (byte)(symbols[1] << 4);
                result |= (byte)(symbols[2] << 2);
                result |= (byte)(symbols[3]);
                return new byte[1] { result };
            }
            return null;
        }
    }
}
