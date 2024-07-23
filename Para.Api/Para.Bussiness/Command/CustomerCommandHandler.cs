using AutoMapper;
using FluentValidation;
using MediatR;
using Para.Base.Response;
using Para.Bussiness.Cqrs;
using Para.Data.Domain;
using Para.Data.UnitOfWork;
using Para.Schema;

public class CreateCustomerCommandHandler : IRequestHandler<CreateCustomerCommand, ApiResponse<CustomerResponse>>
{
    private readonly IUnitOfWork unitOfWork;
    private readonly IMapper mapper;
    private readonly IValidator<Customer> validator;

    public CreateCustomerCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, IValidator<Customer> validator)
    {
        this.unitOfWork = unitOfWork;
        this.mapper = mapper;
        this.validator = validator;
    }

    public async Task<ApiResponse<CustomerResponse>> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = mapper.Map<Customer>(request.Request);
        var validationResult = await validator.ValidateAsync(customer, cancellationToken);

        if (!validationResult.IsValid)
        {
            var errorMessages = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return new ApiResponse<CustomerResponse>(string.Join(", ", errorMessages));
        }

        customer.CustomerNumber = new Random().Next(1000000, 9999999);
        await unitOfWork.CustomerRepository.Insert(customer);
        await unitOfWork.Complete();

        var response = mapper.Map<CustomerResponse>(customer);
        return new ApiResponse<CustomerResponse>(response);
    }

}
