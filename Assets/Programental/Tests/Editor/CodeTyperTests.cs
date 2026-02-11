using NUnit.Framework;

namespace Programental.Tests
{
    public class CodeTyperTests
    {
        [Test]
        public void TypeNextChar_DisparaOnCharTypedConCaracterYTextoVisible()
        {
            var typer = new CodeTyper();
            typer.Initialize();
            char receivedChar = '\0';
            var receivedText = "";
            typer.OnCharTyped += (c, text) =>
            {
                receivedChar = c;
                receivedText = text;
            };

            typer.TypeNextChar();

            Assert.That(receivedChar, Is.Not.EqualTo('\0'), "Debe disparar OnCharTyped con un carácter real");
            Assert.That(receivedText.Length, Is.EqualTo(1), "El texto visible debe tener 1 carácter después del primer TypeNextChar");
            Assert.That(receivedText[0], Is.EqualTo(receivedChar), "El texto visible debe contener el carácter recibido");
        }

        [Test]
        public void TypeNextChar_AlCompletarLinea_DisparaOnLineCompleted()
        {
            var typer = new CodeTyper();
            typer.Initialize();
            var lineCompleted = false;
            var completedLineText = "";
            var completedLineCount = 0;
            typer.OnLineCompleted += (line, count) =>
            {
                lineCompleted = true;
                completedLineText = line;
                completedLineCount = count;
            };

            TypeFullLine(typer);

            Assert.That(lineCompleted, Is.True, "Debe disparar OnLineCompleted cuando la línea se complete");
            Assert.That(completedLineText.Length, Is.GreaterThan(0), "La línea completada debe tener contenido");
            Assert.That(completedLineCount, Is.EqualTo(1), "Debe reportar 1 línea completada");
            Assert.That(typer.LinesCompleted, Is.EqualTo(1), "LinesCompleted debe incrementarse");
        }

        [Test]
        public void TypeNextChar_MultiplesLineas_IncrementaLinesCompleted()
        {
            var typer = new CodeTyper();
            typer.Initialize();

            TypeFullLine(typer);
            TypeFullLine(typer);
            TypeFullLine(typer);

            Assert.That(typer.LinesCompleted, Is.EqualTo(3), "Debe completar 3 líneas correctamente");
        }

        [Test]
        public void MultipleCodeTypers_TipeanIndependientemente()
        {
            var typer1 = new CodeTyper();
            var typer2 = new CodeTyper();
            typer1.Initialize();
            typer2.Initialize();
            var lines1Completed = 0;
            var lines2Completed = 0;
            typer1.OnLineCompleted += (_, count) => lines1Completed = count;
            typer2.OnLineCompleted += (_, count) => lines2Completed = count;

            TypeFullLine(typer1);
            TypeFullLine(typer2);
            TypeFullLine(typer2);

            Assert.That(typer1.LinesCompleted, Is.EqualTo(1), "Typer1 debe haber completado 1 línea");
            Assert.That(typer2.LinesCompleted, Is.EqualTo(2), "Typer2 debe haber completado 2 líneas independientemente");
            Assert.That(lines1Completed, Is.EqualTo(1), "Typer1 debe reportar 1 línea en su evento");
            Assert.That(lines2Completed, Is.EqualTo(2), "Typer2 debe reportar 2 líneas en su evento");
        }

        [Test]
        public void MultipleCodeTypers_EventosNoInterfirenEntreEllos()
        {
            var typer1 = new CodeTyper();
            var typer2 = new CodeTyper();
            typer1.Initialize();
            typer2.Initialize();
            var typer1EventCount = 0;
            var typer2EventCount = 0;
            typer1.OnCharTyped += (_, __) => typer1EventCount++;
            typer2.OnCharTyped += (_, __) => typer2EventCount++;

            typer1.TypeNextChar();
            typer1.TypeNextChar();
            typer2.TypeNextChar();

            Assert.That(typer1EventCount, Is.EqualTo(2), "Typer1 debe haber disparado OnCharTyped 2 veces");
            Assert.That(typer2EventCount, Is.EqualTo(1), "Typer2 debe haber disparado OnCharTyped 1 vez, sin contaminar typer1");
        }

        private void TypeFullLine(CodeTyper typer)
        {
            var maxChars = 200;
            var initialLines = typer.LinesCompleted;
            for (var i = 0; i < maxChars; i++)
            {
                typer.TypeNextChar();
                if (typer.LinesCompleted > initialLines) break;
            }
        }
    }
}
