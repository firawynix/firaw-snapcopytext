using Firaw.SnapCopyText.Services;
using Xunit;

namespace Firaw.SnapCopyText.Tests;

public sealed class SensitiveDataDetectorTests
{
    private readonly SensitiveDataDetector _detector = new();

    [Theory]
    [InlineData("Contato: pessoa@empresa.com.br")]
    [InlineData("CPF 123.456.789-09")]
    [InlineData("CNPJ 12.345.678/0001-90")]
    [InlineData("Telefone (85) 99999-1234")]
    [InlineData("Cartão 4111 1111 1111 1111")]
    [InlineData("Servidor 192.168.0.25")]
    public void IsSensitive_RecognizesProtectedData(string text)
    {
        Assert.True(_detector.IsSensitive(text));
    }

    [Theory]
    [InlineData("")]
    [InlineData("Reunião de projeto amanhã às nove")]
    [InlineData("Pedido 12345 concluído")]
    [InlineData("Cartão inválido 4111 1111 1111 1112")]
    [InlineData("Endereço inválido 999.999.999.999")]
    public void IsSensitive_IgnoresOrdinaryOrInvalidText(string text)
    {
        Assert.False(_detector.IsSensitive(text));
    }
}
