    document.addEventListener('DOMContentLoaded', function () {
            var updateForm = document.getElementById('update');
        if(updateForm){
                updateForm.addEventListener('submit', function (e) {
                    e.preventDefault();
                     var confirmed = confirm('Confirm Status Update? Delivered or Cancelled Status cannot be reverted.');
                    if (confirmed) {
                        updateForm.submit();
                    }
                });
            };
        });

     