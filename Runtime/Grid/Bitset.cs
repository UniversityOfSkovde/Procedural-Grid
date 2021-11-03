namespace Grid {
    public static class Bitset {
        public static bool Get(uint bitset, int property) {
            return 0u != (bitset & (1u << property));
        }

        public static void Set(ref uint bitset, int property, bool value) {
            if (value) {
                bitset |= 1u << property;
            } else {
                bitset &= ~(1u << property);
            }
        }
    }
}