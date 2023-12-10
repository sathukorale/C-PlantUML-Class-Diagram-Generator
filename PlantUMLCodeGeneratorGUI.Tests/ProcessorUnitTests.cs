using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PlantUMLCodeGeneratorGUI.classes.exceptions;

namespace PlantUMLCodeGeneratorGUI.Tests
{
    [TestClass]
    public class ProcessorUnitTests
    {
        [TestMethod]
        public void FindContainedTypes_Combination1()
        {
            var result = CodeProcessor.FindContainedTypes("int");
            Assert.AreEqual(result.Count, 1);

            var firstResult = result[0];
            Assert.IsFalse(firstResult is CodeProcessor.TemplateType);
            Assert.AreEqual(firstResult.Type, "int");
        }

        [TestMethod]
        public void FindContainedTypes_Combination2()
        {
            var result = CodeProcessor.FindContainedTypes("std::string");
            Assert.AreEqual(result.Count, 1);

            var firstResult = result[0];
            Assert.IsFalse(firstResult is CodeProcessor.TemplateType);
            Assert.AreEqual(firstResult.Type, "std::string");
        }

        [TestMethod]
        public void FindContainedTypes_Combination3()
        {
            var result = CodeProcessor.FindContainedTypes("int, std::unique_ptr<Something>");
            Assert.AreEqual(result.Count, 2);

            var firstResult = result[0];
            var secondResult = result[1];
            Assert.IsFalse(firstResult is CodeProcessor.TemplateType);
            Assert.IsTrue(secondResult is CodeProcessor.TemplateType);

            Assert.AreEqual(firstResult.Type, "int");
            Assert.AreEqual(secondResult.Type, "std::unique_ptr");

            var thirdResults = (secondResult as CodeProcessor.TemplateType).TemplateTypes;
            Assert.AreEqual(thirdResults.Count, 1);

            var thirdResult = thirdResults[0];
            Assert.IsFalse(thirdResult is CodeProcessor.TemplateType);
            Assert.AreEqual(thirdResult.Type, "Something");
        }

        [TestMethod]
        public void FindContainedTypes_Combination4()
        {
            var result = CodeProcessor.FindContainedTypes("int, std::string");
            Assert.AreEqual(result.Count, 2);

            var firstResult = result[0];
            var secondResult = result[1];

            Assert.IsFalse(firstResult is CodeProcessor.TemplateType);
            Assert.IsFalse(secondResult is CodeProcessor.TemplateType);
            Assert.AreEqual(firstResult.Type, "int");
            Assert.AreEqual(secondResult.Type, "std::string");
        }

        [TestMethod]
        public void FindContainedTypes_Combination5()
        {
            var result = CodeProcessor.FindContainedTypes("int, Something<int, int, std::string>");
            Assert.AreEqual(result.Count, 2);
            var firstResult = result[0];
            var secondResult = result[1];

            Assert.IsFalse(firstResult is CodeProcessor.TemplateType);
            Assert.IsTrue(secondResult is CodeProcessor.TemplateType);

            Assert.AreEqual(firstResult.Type, "int");
            Assert.AreEqual(secondResult.Type, "Something");

            var thirdResults = (secondResult as CodeProcessor.TemplateType).TemplateTypes;
            Assert.AreEqual(thirdResults.Count, 3);

            var thirdResult = thirdResults[0];
            var fourthResult = thirdResults[1];
            var fifthResult = thirdResults[2];

            Assert.IsFalse(thirdResult is CodeProcessor.TemplateType);
            Assert.IsFalse(fourthResult is CodeProcessor.TemplateType);
            Assert.IsFalse(fifthResult is CodeProcessor.TemplateType);
        }

        [TestMethod]
        public void FindContainedTypes_Combination6()
        {
            // TODO: This test case seems to be taking a long time compared to the others. Investigate why.
            var result = CodeProcessor.FindContainedTypes("int, Something<int, int, Something<int, int, std::string>>");
            Assert.AreEqual(result.Count, 2);

            var firstResult = result[0];
            var secondResult = result[1];

            Assert.IsFalse(firstResult is CodeProcessor.TemplateType);
            Assert.IsTrue(secondResult is CodeProcessor.TemplateType);

            Assert.AreEqual(firstResult.Type, "int");
            Assert.AreEqual(secondResult.Type, "Something");

            var thirdResults = (secondResult as CodeProcessor.TemplateType).TemplateTypes;
            Assert.AreEqual(thirdResults.Count, 3);

            var thirdResult = thirdResults[0];
            var fourthResult = thirdResults[1];
            var fifthResult = thirdResults[2];

            Assert.IsFalse(thirdResult is CodeProcessor.TemplateType);
            Assert.IsFalse(fourthResult is CodeProcessor.TemplateType);
            Assert.IsTrue(fifthResult is CodeProcessor.TemplateType);

            var sixthResults = (fifthResult as CodeProcessor.TemplateType).TemplateTypes;
            Assert.AreEqual(sixthResults.Count, 3);

            var sixthResult = sixthResults[0];
            var seventhResult = sixthResults[1];
            var eighthResult = sixthResults[2];

            Assert.IsFalse(sixthResult is CodeProcessor.TemplateType);
            Assert.IsFalse(seventhResult is CodeProcessor.TemplateType);
            Assert.IsFalse(eighthResult is CodeProcessor.TemplateType);

            Assert.AreEqual(sixthResult.Type, "int");
            Assert.AreEqual(seventhResult.Type, "int");
            Assert.AreEqual(eighthResult.Type, "std::string");
        }

        [TestMethod]
        public void FindContainedTypes_Combination7()
        {
            var result = CodeProcessor.FindContainedTypes("Something<int, int, std::string>, int");
            Assert.AreEqual(result.Count, 2);

            var firstResult = result[0];
            var secondResult = result[1];
                
            Assert.IsTrue(firstResult is CodeProcessor.TemplateType);
            Assert.IsFalse(secondResult is CodeProcessor.TemplateType);

            Assert.AreEqual(firstResult.Type, "Something");
            Assert.AreEqual(secondResult.Type, "int");

            var thirdResults = (firstResult as CodeProcessor.TemplateType).TemplateTypes;
            Assert.AreEqual(thirdResults.Count, 3);

            var thirdResult = thirdResults[0];
            var fourthResult = thirdResults[1];
            var fifthResult = thirdResults[2];

            Assert.IsFalse(thirdResult is CodeProcessor.TemplateType);
            Assert.IsFalse(fourthResult is CodeProcessor.TemplateType);
            Assert.IsFalse(fifthResult is CodeProcessor.TemplateType);
        }

        [TestMethod]
        public void FindContainedTypes_Combination8()
        {
            var result = CodeProcessor.FindContainedTypes("Something<int, int, Something<int, int, std::string>>, int");
            Assert.AreEqual(result.Count, 2);

            var firstResult = result[0];
            var secondResult = result[1];

            Assert.IsTrue(firstResult is CodeProcessor.TemplateType);
            Assert.IsFalse(secondResult is CodeProcessor.TemplateType);

            Assert.AreEqual(firstResult.Type, "Something");
            Assert.AreEqual(secondResult.Type, "int");

            var thirdResults = (firstResult as CodeProcessor.TemplateType).TemplateTypes;
            Assert.AreEqual(thirdResults.Count, 3);

            var thirdResult = thirdResults[0];
            var fourthResult = thirdResults[1];
            var fifthResult = thirdResults[2];

            Assert.IsFalse(thirdResult is CodeProcessor.TemplateType);
            Assert.IsFalse(fourthResult is CodeProcessor.TemplateType);
            Assert.IsTrue(fifthResult is CodeProcessor.TemplateType);

            Assert.AreEqual(thirdResult.Type, "int");
            Assert.AreEqual(fourthResult.Type, "int");
            Assert.AreEqual(fifthResult.Type, "Something");

            var sixthResults = (fifthResult as CodeProcessor.TemplateType).TemplateTypes;
            Assert.AreEqual(sixthResults.Count, 3);

            var sixthResult = sixthResults[0];
            var seventhResult = sixthResults[1];
            var eighthResult = sixthResults[2];

            Assert.IsFalse(sixthResult is CodeProcessor.TemplateType);
            Assert.IsFalse(seventhResult is CodeProcessor.TemplateType);
            Assert.IsFalse(eighthResult is CodeProcessor.TemplateType);

            Assert.AreEqual(sixthResult.Type, "int");
            Assert.AreEqual(seventhResult.Type, "int");
            Assert.AreEqual(eighthResult.Type, "std::string");
        }

        [TestMethod]
        public void FindContainedTypes_Combination9()
        {
            var result = CodeProcessor.FindContainedTypes("Something<int, int, std::string>, Something<int, int, std::string>");
            Assert.AreEqual(result.Count, 2);

            var firstResult = result[0];
            var secondResult = result[1];

            Assert.IsTrue(firstResult is CodeProcessor.TemplateType);
            Assert.IsTrue(secondResult is CodeProcessor.TemplateType);

            Assert.AreEqual(firstResult.Type, "Something");
            Assert.AreEqual(secondResult.Type, "Something");

            var thirdResults = (firstResult as CodeProcessor.TemplateType).TemplateTypes;
            Assert.AreEqual(thirdResults.Count, 3);

            var thirdResult = thirdResults[0];
            var fourthResult = thirdResults[1];
            var fifthResult = thirdResults[2];

            Assert.IsFalse(thirdResult is CodeProcessor.TemplateType);
            Assert.IsFalse(fourthResult is CodeProcessor.TemplateType);
            Assert.IsFalse(fifthResult is CodeProcessor.TemplateType);

            Assert.AreEqual(thirdResult.Type, "int");
            Assert.AreEqual(fourthResult.Type, "int");
            Assert.AreEqual(fifthResult.Type, "std::string");

            var sixthResults = (secondResult as CodeProcessor.TemplateType).TemplateTypes;
            Assert.AreEqual(sixthResults.Count, 3);
            
            var sixthResult = sixthResults[0];
            var seventhResult = sixthResults[1];
            var eighthResult = sixthResults[2];

            Assert.IsFalse(sixthResult is CodeProcessor.TemplateType);
            Assert.IsFalse(seventhResult is CodeProcessor.TemplateType);
            Assert.IsFalse(eighthResult is CodeProcessor.TemplateType);

            Assert.AreEqual(sixthResult.Type, "int");
            Assert.AreEqual(seventhResult.Type, "int");
            Assert.AreEqual(eighthResult.Type, "std::string");
        }

        [TestMethod]
        public void FindContainedTypes_Combination10()
        {
            var result = CodeProcessor.FindContainedTypes("Something<int, int, Something<int, int, std::string>>, Something<int, int, Something<int, int, std::string>>");
            Assert.AreEqual(result.Count, 2);

            var firstResult = result[0];
            var secondResult = result[1];

            Assert.IsTrue(firstResult is CodeProcessor.TemplateType);
            Assert.IsTrue(secondResult is CodeProcessor.TemplateType);

            Assert.AreEqual(firstResult.Type, "Something");
            Assert.AreEqual(secondResult.Type, "Something");

            var thirdResults = (firstResult as CodeProcessor.TemplateType).TemplateTypes;
            Assert.AreEqual(thirdResults.Count, 3);

            var thirdResult = thirdResults[0];
            var fourthResult = thirdResults[1];
            var fifthResult = thirdResults[2];

            Assert.IsFalse(thirdResult is CodeProcessor.TemplateType);
            Assert.IsFalse(fourthResult is CodeProcessor.TemplateType);
            Assert.IsTrue(fifthResult is CodeProcessor.TemplateType);

            Assert.AreEqual(thirdResult.Type, "int");
            Assert.AreEqual(fourthResult.Type, "int");
            Assert.AreEqual(fifthResult.Type, "Something");

            var sixthResults = (fifthResult as CodeProcessor.TemplateType).TemplateTypes;
            Assert.AreEqual(sixthResults.Count, 3);

            var sixthResult = sixthResults[0];
            var seventhResult = sixthResults[1];
            var eighthResult = sixthResults[2];

            Assert.IsFalse(sixthResult is CodeProcessor.TemplateType);
            Assert.IsFalse(seventhResult is CodeProcessor.TemplateType);
            Assert.IsFalse(eighthResult is CodeProcessor.TemplateType);

            Assert.AreEqual(sixthResult.Type, "int");
            Assert.AreEqual(seventhResult.Type, "int");
            Assert.AreEqual(eighthResult.Type, "std::string");

            var ninthResults = (secondResult as CodeProcessor.TemplateType).TemplateTypes;
            Assert.AreEqual(ninthResults.Count, 3);
            
            var ninthResult = ninthResults[0];
            var tenthResult = ninthResults[1];
            var eleventhResult = ninthResults[2];

            Assert.IsFalse(ninthResult is CodeProcessor.TemplateType);
            Assert.IsFalse(tenthResult is CodeProcessor.TemplateType);
            Assert.IsTrue(eleventhResult is CodeProcessor.TemplateType);

            Assert.AreEqual(ninthResult.Type, "int");
            Assert.AreEqual(tenthResult.Type, "int");
            Assert.AreEqual(eleventhResult.Type, "Something");

            var twelfthResults = (eleventhResult as CodeProcessor.TemplateType).TemplateTypes;
            Assert.AreEqual(twelfthResults.Count, 3);

            var twelfthResult = twelfthResults[0];
            var thirteenthResult = twelfthResults[1];
            var fourteenthResult = twelfthResults[2];

            Assert.IsFalse(twelfthResult is CodeProcessor.TemplateType);
            Assert.IsFalse(thirteenthResult is CodeProcessor.TemplateType);
            Assert.IsFalse(fourteenthResult is CodeProcessor.TemplateType);

            Assert.AreEqual(twelfthResult.Type, "int");
            Assert.AreEqual(thirteenthResult.Type, "int");
            Assert.AreEqual(fourteenthResult.Type, "std::string");
        }

        [TestMethod]
        public void GetScopedContent_WithEmptyString_ShouldReturnEmptyString()
        {
            var input = "";
            var offset = 0;

            var result = CodeProcessor.GetScopedContent(input, ref offset);

            Assert.AreEqual("", result);
        }

        [TestMethod]
        public void GetScopedContent_WithBracesScope_ShouldExtractContent()
        {
            var input = "Prefix {scoped content} Postfix";
            var offset = 0;
            var expected = "scoped content";

            var result = CodeProcessor.GetScopedContent(input, ref offset, CodeProcessor.ScopeCharacterType.Braces);

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void GetScopedContent_WithAngleBracketsScope_ShouldExtractContent()
        {
            var input = "Prefix <scoped content> Postfix";
            var offset = 0;
            var expected = "scoped content";

            var result = CodeProcessor.GetScopedContent(input, ref offset, CodeProcessor.ScopeCharacterType.AngleBrackets);

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void GetScopedContent_WithParenthesesScope_ShouldExtractContent()
        {
            var input = "Prefix (scoped content) Postfix";
            var offset = 0;
            var expected = "scoped content";

            var result = CodeProcessor.GetScopedContent(input, ref offset, CodeProcessor.ScopeCharacterType.Parentheses);

            Assert.AreEqual(expected, result);
        }


        [TestMethod]
        public void GetScopedContent_WithNestedScopes_ShouldExtractOuterContent()
        {
            var input = "Prefix {Outer {Inner} Outer} Postfix";
            var offset = 0;
            var expected = "Outer {Inner} Outer";

            var result = CodeProcessor.GetScopedContent(input, ref offset, CodeProcessor.ScopeCharacterType.Braces);

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void GetScopedContent_WithMissingClosingScope_ShouldThrowException()
        {
            var input = "Prefix {No closing";
            var offset = 0;

            Assert_Throws<ScopeNotClosedException>(() =>
                CodeProcessor.GetScopedContent(input, ref offset, CodeProcessor.ScopeCharacterType.Braces));
        }

        [TestMethod]
        public void GetScopedContent_WithMultipleScopes_ShouldExtractFirstCompleteScope()
        {
            var input = "{First} {Second}";
            var offset = 0;
            var expected = "First";

            var result = CodeProcessor.GetScopedContent(input, ref offset, CodeProcessor.ScopeCharacterType.Braces);

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void GetScopedContent_ShouldUpdateOffsetCorrectly()
        {
            var input = "Prefix {scoped content} Postfix";
            var offset = 0;
            var expectedOffset = "Prefix ".Length + "scoped content".Length + 2; // 2 for the braces

            CodeProcessor.GetScopedContent(input, ref offset, CodeProcessor.ScopeCharacterType.Braces);

            Assert.AreEqual(expectedOffset, offset);
        }

        [TestMethod]
        public void GetScopedContent_WithMixedNestedScopes_ShouldExtractContentCorrectly()
        {
            var input = "Prefix {Outer (Inner <Deep>) Outer} Postfix";
            var offset = 0;
            var expected = "Outer (Inner <Deep>) Outer";

            var result = CodeProcessor.GetScopedContent(input, ref offset, CodeProcessor.ScopeCharacterType.Braces);

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void GetScopedContent_WithMixedNestedScopes_ShouldExtractTheInnerContentCorrectly()
        {
            var input = "Prefix {Outer (Inner <Deep>) Outer} Postfix";
            var offset = 0;
            var expected = "Outer (Inner <Deep>) Outer";

            var result = CodeProcessor.GetScopedContent(input, ref offset, CodeProcessor.ScopeCharacterType.Braces);

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void GetScopedContent_WithUnrelatedUnbalancedScopes_ShouldExtractTheRelevantScope()
        {
            var input = "Prefix {Outer (Inner} Postfix";
            var offset = 0;
            var expected = "Outer (Inner";

            var result = CodeProcessor.GetScopedContent(input, ref offset, CodeProcessor.ScopeCharacterType.Braces);

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void GetScopedContent_WithDeeplyNestedScopes_ShouldExtractOuterContent()
        {
            var input = "{First {Second {Third} Second} First}";
            var offset = 0;
            var expected = "First {Second {Third} Second} First";

            var result = CodeProcessor.GetScopedContent(input, ref offset, CodeProcessor.ScopeCharacterType.Braces);

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void GetScopedContent_WithDeeplyNestedScopes_WithOffset_ShouldExtractInnerContent()
        {
            var input = "{First {Second {Third} Second} First}";
            var offset = 3;
            var expected = "Second {Third} Second";

            var result = CodeProcessor.GetScopedContent(input, ref offset, CodeProcessor.ScopeCharacterType.Braces);

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void GetScopedContent_WithScopesOnSameLevel_ShouldExtractFirstScope()
        {
            var input = "{First} {Second}";
            var offset = 0;
            var expected = "First";

            var result = CodeProcessor.GetScopedContent(input, ref offset, CodeProcessor.ScopeCharacterType.Braces);

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void GetScopedContent_WithInnerScopesOnSameLevel_ShouldExtractContentCorrectly()
        {
            var input = "{Outer {Inner1} {Inner2} Outer}";
            var offset = 0;
            var expected = "Outer {Inner1} {Inner2} Outer";

            var result = CodeProcessor.GetScopedContent(input, ref offset, CodeProcessor.ScopeCharacterType.Braces);

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void GetScopedContent_WithComplexNestedScopes_ShouldExtractContentCorrectly()
        {
            var input = "{Outer <Mid {Deep (Deeper)} Mid> Outer}";
            var offset = 0;
            var expected = "Outer <Mid {Deep (Deeper)} Mid> Outer";

            var result = CodeProcessor.GetScopedContent(input, ref offset, CodeProcessor.ScopeCharacterType.Braces);

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void GetScopedContent_WithNonAlphanumericCharacters_ShouldExtractContentCorrectly()
        {
            var input = "{@#$%^&*()!}";
            var offset = 0;
            var expected = "@#$%^&*()!";

            var result = CodeProcessor.GetScopedContent(input, ref offset, CodeProcessor.ScopeCharacterType.Braces);

            Assert.AreEqual(expected, result);
        }

        public static T Assert_Throws<T> (Action action) where T : Exception
        {
            try
            {
                action.Invoke();
                Assert.Fail($"Expected exception of type '{typeof(T).FullName}' but did not throw.");
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is T);
                return (T)e;
            }

            return null;
        }
    }
}
