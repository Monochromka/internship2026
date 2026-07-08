using System.ComponentModel.DataAnnotations;
using Projects.Api.Models; 
using Xunit;

namespace Projects.Api.Tests
{
    public class CreateProjectDtoTests
    {
        [Fact]
        public void CreateProjectDto_WithValidData_ShouldPassValidation()
        {
            var dto = new CreateProjectDto
            {
                Name = "Test Project",
                Description = "This is a valid description."
            };

            var validationContext = new ValidationContext(dto);
            var validationResults = new List<ValidationResult>();


            bool isValid = Validator.TryValidateObject(dto, validationContext, validationResults, true);

            Assert.True(isValid);
            Assert.Empty(validationResults);
        }


        [Fact]
        public void CreateProjectDto_WithNameExceedsMaxLength_ShouldFailValidation()
        {
            var dto = new CreateProjectDto
            {
                Name = new string('A', 101),
                Description = "Valid description"
            };

            var validationContext = new ValidationContext(dto);
            var validationResults = new List<ValidationResult>();

            bool isValid = Validator.TryValidateObject(dto, validationContext, validationResults, true);

            Assert.False(isValid); 
            Assert.Contains(validationResults, v => v.MemberNames.Contains("Name"));
        }

        [Fact]
        public void CreateProjectDto_WithDescriptionExceedsMaxLength_ShouldFailValidation()
        {
            var dto = new CreateProjectDto
            {
                Name = "Valid Name",
                Description = new string('B', 501)
            };

            var validationContext = new ValidationContext(dto);
            var validationResults = new List<ValidationResult>();

            bool isValid = Validator.TryValidateObject(dto, validationContext, validationResults, true);

            Assert.False(isValid);
            Assert.Contains(validationResults, v => v.MemberNames.Contains("Description"));
        }
    }
}