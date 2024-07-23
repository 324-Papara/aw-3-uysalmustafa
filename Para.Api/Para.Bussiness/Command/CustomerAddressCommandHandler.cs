using AutoMapper;
using FluentValidation;
using MediatR;
using Para.Base.Response;
using Para.Bussiness.Cqrs;
using Para.Data.Domain;
using Para.Data.UnitOfWork;
using Para.Schema;

namespace Para.Bussiness.Command
{
    public class CustomerAddressCommandHandler :
        IRequestHandler<CreateCustomerAddressCommand, ApiResponse<CustomerAddressResponse>>,
        IRequestHandler<UpdateCustomerAddressCommand, ApiResponse>,
        IRequestHandler<DeleteCustomerAddressCommand, ApiResponse>
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;
        private readonly IValidator<CustomerAddress> validator;

        public CustomerAddressCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, IValidator<CustomerAddress> validator)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
            this.validator = validator;
        }

        public async Task<ApiResponse<CustomerAddressResponse>> Handle(CreateCustomerAddressCommand request, CancellationToken cancellationToken)
        {
            var customerAddress = mapper.Map<CustomerAddressRequest, CustomerAddress>(request.Request);

 
            var validationResult = await validator.ValidateAsync(customerAddress, cancellationToken);

            if (!validationResult.IsValid)
            {
                var errorMessages = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                return new ApiResponse<CustomerAddressResponse>(string.Join(", ", errorMessages));
            }

            await unitOfWork.CustomerAddressRepository.Insert(customerAddress);
            await unitOfWork.Complete();

            var response = mapper.Map<CustomerAddressResponse>(customerAddress);
            return new ApiResponse<CustomerAddressResponse>(response);
        }

        public async Task<ApiResponse> Handle(UpdateCustomerAddressCommand request, CancellationToken cancellationToken)
        {
            var customerAddress = mapper.Map<CustomerAddressRequest, CustomerAddress>(request.Request);
            customerAddress.Id = request.AddressId;

            var validationResult = await validator.ValidateAsync(customerAddress, cancellationToken);

            if (!validationResult.IsValid)
            {
                var errorMessages = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                return new ApiResponse(string.Join(", ", errorMessages));
            }

            unitOfWork.CustomerAddressRepository.Update(customerAddress);
            await unitOfWork.Complete();
            return new ApiResponse();
        }

        public async Task<ApiResponse> Handle(DeleteCustomerAddressCommand request, CancellationToken cancellationToken)
        {
            await unitOfWork.CustomerAddressRepository.Delete(request.AddressId);
            await unitOfWork.Complete();
            return new ApiResponse();
        }
    }
}
