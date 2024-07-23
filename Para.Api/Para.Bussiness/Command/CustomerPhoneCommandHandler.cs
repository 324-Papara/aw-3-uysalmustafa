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
    public class CustomerPhoneCommandHandler :
        IRequestHandler<CreateCustomerPhoneCommand, ApiResponse<CustomerPhoneResponse>>,
        IRequestHandler<UpdateCustomerPhoneCommand, ApiResponse>,
        IRequestHandler<DeleteCustomerPhoneCommand, ApiResponse>
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;
        private readonly IValidator<CustomerPhone> validator;

        public CustomerPhoneCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, IValidator<CustomerPhone> validator)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
            this.validator = validator;
        }

        public async Task<ApiResponse<CustomerPhoneResponse>> Handle(CreateCustomerPhoneCommand request, CancellationToken cancellationToken)
        {
            var customerPhone = mapper.Map<CustomerPhoneRequest, CustomerPhone>(request.Request);

            var validationResult = await validator.ValidateAsync(customerPhone, cancellationToken);

            if (!validationResult.IsValid)
            {
                var errorMessages = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                return new ApiResponse<CustomerPhoneResponse>(string.Join(", ", errorMessages));
            }

            await unitOfWork.CustomerPhoneRepository.Insert(customerPhone);
            await unitOfWork.Complete();

            var response = mapper.Map<CustomerPhoneResponse>(customerPhone);
            return new ApiResponse<CustomerPhoneResponse>(response);
        }

        public async Task<ApiResponse> Handle(UpdateCustomerPhoneCommand request, CancellationToken cancellationToken)
        {
            var customerPhone = mapper.Map<CustomerPhoneRequest, CustomerPhone>(request.Request);
            customerPhone.Id = request.PhoneId;

            var validationResult = await validator.ValidateAsync(customerPhone, cancellationToken);

            if (!validationResult.IsValid)
            {
                var errorMessages = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                return new ApiResponse(string.Join(", ", errorMessages));
            }

            unitOfWork.CustomerPhoneRepository.Update(customerPhone);
            await unitOfWork.Complete();
            return new ApiResponse();
        }

        public async Task<ApiResponse> Handle(DeleteCustomerPhoneCommand request, CancellationToken cancellationToken)
        {
            await unitOfWork.CustomerPhoneRepository.Delete(request.PhoneId);
            await unitOfWork.Complete();
            return new ApiResponse();
        }
    }
}
