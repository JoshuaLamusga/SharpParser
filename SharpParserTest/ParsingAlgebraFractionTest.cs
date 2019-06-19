using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpParser.Parsing.Algebra;

namespace SharpParserTest
{
    [TestClass]
    public class ParsingAlgebraFractionTest
    {
        #region Constructor
        [TestMethod]
        public void Constructor_PositiveFractionsSimplify()
        {
            //Positive fraction simplification.
            Fraction frac = new Fraction(2, 4);
            Assert.AreEqual(frac.Numerator, 1);
            Assert.AreEqual(frac.Denominator, 2);
        }

        [TestMethod]
        public void Constructor_PositiveNegativeFractionsSimplify()
        {
            //Negative fraction simplification.
            Fraction frac2 = new Fraction(2, -4);
            Assert.AreEqual(frac2.Numerator, -1);
            Assert.AreEqual(frac2.Denominator, 2);
        }

        [TestMethod]
        public void Constructor_NegativeFractionsSimplify()
        {
            //Double-negative simplification.
            Fraction frac3 = new Fraction(-4, -2);
            Assert.AreEqual(frac3.Numerator, 2);
            Assert.AreEqual(frac3.Denominator, 1);
        }
        #endregion        

        #region Add
        [TestMethod]
        public void Add_PositiveFractionsAdd()
        {
            Fraction frac = new Fraction(1, 2);
            Fraction frac2 = new Fraction(1, 3);
            Fraction frac3 = new Fraction(0, 3);
            Assert.AreEqual(Fraction.Add(frac, frac2).GetValue(), 5 / 6m);
            Assert.AreEqual(Fraction.Add(frac, frac3).GetValue(), 1 / 2m);
            Assert.AreEqual(Fraction.Add(new LiteralNum(2), frac).GetValue(), 2.5m);
            Assert.AreEqual(Fraction.Add(frac, new LiteralNum(2)).GetValue(), 2.5m);
        }

        [TestMethod]
        public void Add_HandlesNegativeFractions()
        {
            Fraction frac1 = new Fraction(1, 3);
            Fraction frac2 = new Fraction(-1, 2);
            Fraction frac3 = new Fraction(-1, 3);
            Assert.AreEqual(Fraction.Add(frac2, frac1).GetValue(), -1 / 6m);
            Assert.AreEqual(Fraction.Add(frac2, frac3).GetValue(), -5 / 6m);
        }

        [TestMethod]
        [ExpectedException(typeof(ParsingException), "Attempted to divide by zero.")]
        public void Add_HandlesUndefinedFractions()
        {
            Fraction frac = new Fraction(1, 2);
            Fraction frac2 = new Fraction(0, 0);
            Fraction.Add(frac, frac2).GetValue();
        }
        #endregion

        #region Divide
        [TestMethod]
        public void Divide_PositiveFractionsDivide()
        {
            Fraction frac = new Fraction(1, 2);
            Fraction frac2 = new Fraction(1, 3);
            Assert.AreEqual(Fraction.Divide(frac, frac).GetValue(), 1m);
            Assert.AreEqual(Fraction.Divide(frac, frac2).GetValue(), 1.5m);
        }

        [TestMethod]
        public void Divide_HandlesNegativeFractions()
        {
            Fraction frac = new Fraction(1, 2);
            Fraction frac2 = new Fraction(1, 3);
            Assert.AreEqual(Fraction.Divide(frac, frac).GetValue(), 1m);
            Assert.AreEqual(Fraction.Divide(frac, frac2).GetValue(), 1.5m);
        }

        [TestMethod]
        public void Divide_HandlesUndefinedFractions()
        {
            Fraction frac = new Fraction(1, 0);
            Fraction frac2 = new Fraction(1, 3);
            Fraction.Divide(frac, frac2).GetValue();
            Assert.AreEqual(Fraction.Divide(frac, frac2).GetValue(), 1.5m);
        }
        #endregion

        #region Equals
        [TestMethod]
        public void Equals_EquivalentFractionsAreEqual()
        {
            Fraction frac = new Fraction(2, 1);
            Fraction frac2 = new Fraction(2, 1);
            Assert.IsTrue(frac.Equals(frac2));
        }

        [TestMethod]
        public void Equals_InequalFractionsAreInequal()
        {
            Fraction frac = new Fraction(2, 1);
            Fraction frac2 = new Fraction(2, 2);
            Assert.IsFalse(frac.Equals(frac2));
        }

        [TestMethod]
        public void Equals_InequalToNull()
        {
            Fraction frac = new Fraction(2, 1);
            Assert.IsFalse(frac.Equals(null));
        }
        #endregion

        #region GetValue
        [TestMethod]
        [ExpectedException(typeof(ParsingException), "Attempted to divide by zero.")]
        public void GetValue_UndefinedFractionsFail()
        {
            new Fraction(1, 0).GetValue();
        }
        #endregion
    }
}
