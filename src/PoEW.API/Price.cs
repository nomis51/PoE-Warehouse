using PoEW.Data.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace PoEW.Data {
    public class Price {
        public CurrencyType Currency { get; set; }
        public PriceType Type { get; set; }
        public float Value { get; set; } = 1.0f;

        public Price(CurrencyType currency, PriceType type, float value) {
            Currency = currency;
            Type = type;
            Value = value;
        }

        public override string ToString() {
            return $"{Shop.PriceTypeToTag[Type]} {Value} {Shop.CurrencyTypeToString[Currency]}";
        }
    }
}
