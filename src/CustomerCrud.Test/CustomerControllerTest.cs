using CustomerCrud.Core;
using CustomerCrud.Repositories;
using CustomerCrud.Requests;

namespace CustomerCrud.Test;

public class CustomersControllerTest : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<ICustomerRepository> _repositoryMock;

    public CustomersControllerTest(WebApplicationFactory<Program> factory)
    {
        _repositoryMock = new Mock<ICustomerRepository>();

        _client = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddScoped<ICustomerRepository>(st => _repositoryMock.Object);
            });
        }).CreateClient();
    }

    [Fact]
    public async Task GetAllTest()
    {
        var customers = AutoFaker.Generate<Customer>(3);

        _repositoryMock
            .Setup(customer => customer.GetAll())
            .Returns(customers.AsQueryable());

        var res = await _client.GetAsync("/controller");
        var content = await res.Content.ReadFromJsonAsync<IEnumerable<Customer>>();

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().BeEquivalentTo(customers);

        _repositoryMock.Verify(db => db.GetAll(), Times.Once);
    }

    [Fact]
    public async Task GetByIdTest()
    {
        List<Customer> customer = (List<Customer>)AutoFaker.Generate<Customer>(1);

        customer.ElementAt(0).Id = 1;
        _repositoryMock
            .Setup(customer => customer.GetById(1))
            .Returns(customer.ElementAt(0));

        var res = await _client.GetAsync("/controller/1");
        var content = await res.Content.ReadFromJsonAsync<Customer>();

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().BeEquivalentTo(customer.ElementAt(0));

        _repositoryMock.Verify(db => db.GetById(1), Times.Once);
    }

    [Fact]
    public async Task CreateTest()
    {
        var customerRequest = AutoFaker.Generate<CustomerRequest>(1);

        _repositoryMock
            .Setup(customer => customer.GetNextIdValue())
            .Returns(1);
        _repositoryMock
            .Setup(customer => customer.Create(
                It.Is<Customer>(r => r.Id == 1))
            ).Returns(true);
        
        var res = await _client.PostAsJsonAsync("/controller", customerRequest[0]);
        var content = await res.Content.ReadFromJsonAsync<Customer>();

        res.StatusCode.Should().Be(HttpStatusCode.Created);
        
    //     content.Name.Should().Be(customerRequest[0].Name);
    //     content.CPF.Should().Be(customerRequest[0].CPF);
    //     content.Transactions.Should().BeEquivalentTo(customerRequest[0].Transactions);

    // Ver um pouco mais...
    //     _repositoryMock.Verify(db => db.GetNextIdValue(), Times.Once);
    //     _repositoryMock.Verify(db => db.Create(It.Is<Customer>(r => r.Id == 1)), Times.Once);
    }

    [Fact]
    public async Task UpdateTest()
    {
        var customerRequest = AutoFaker.Generate<CustomerRequest>(1);
        _repositoryMock.Setup(st => st.Update(1, It.IsAny<Object>())).Returns(true);
        
        var res = await _client.PutAsJsonAsync("/controller/1", customerRequest[0]);
        var content = await res.Content.ReadAsStringAsync();

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().Be("Customer 1 updated");

        // _repositoryMock.Verify(db => db.Update(1, It.IsAny<Object>()), Times.Once);
    }

    [Fact]
    public async Task DeleteTest()
    {
        _repositoryMock.SetupSequence(st => st.Delete(1))
            .Returns(true);
        
        var res = await _client.DeleteAsync("/controller/1");
        var content = await res.Content.ReadAsStringAsync();

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().Be("Customer 1 deleted");

        // Caso de erro...
        // var resError = await _client.DeleteAsync("/controller/1");
        // var contentError = await res.Content.ReadAsStringAsync();

        // resError.StatusCode.Should().Be(HttpStatusCode.NotFound);
        // contentError.Should().Be("Customer Not Found");
    }
}