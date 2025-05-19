using System.Diagnostics;
using KVDbDemo;
using KVDbDemo.Storage;
using Moq;

namespace TestKVDbDemo;

[TestClass]
public class TestService
{
    [TestMethod]
    public void TestDispatchWithValidPath()
    {
        const string path       = "add";
        const string body       = "sample body";
        const string response   = "sample response";
        
        Service service         = new Service("localhost", 5678);
        var mockStorage         = new Mock<IStorage>();
        var mockHandleRequest   = new Mock<IHandleRequest>();

        mockHandleRequest.Setup(m => m.OnRequest(It.IsAny<IStorage>(), It.IsAny<string>()))
            .Returns(response);
            
        service.MapRequest(path, mockHandleRequest.Object);
        string? result = service.OnDispatch(mockStorage.Object, path, body);
        Assert.IsTrue(result != null);
        Assert.AreEqual(result, response);
        
        mockHandleRequest.Verify(m => m.OnRequest(
            It.IsAny<IStorage>(), 
              It.Is<string>(v => v == body))
        );
        
        mockHandleRequest.Verify(m => m.OnRequest(It.IsAny<IStorage>(), It.IsAny<string>()), 
            Times.Once);
    }
    
    [TestMethod]
    public void TestDispatchWithInValidPath()
    {
        Service service         = new Service("localhost", 5678);
        var mockStorage         = new Mock<IStorage>();
        var mockHandleRequest   = new Mock<IHandleRequest>();
        
        string? result = service.OnDispatch(mockStorage.Object, "invalid", "");
        Assert.IsTrue(result == null);
        
        mockHandleRequest.Verify(m => m.OnRequest(It.IsAny<IStorage>(), It.IsAny<string>()), 
            Times.Never);
    }
}