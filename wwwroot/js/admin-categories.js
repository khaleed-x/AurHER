    document.addEventListener('DOMContentLoaded', function () {
            var deleteForms = document.querySelectorAll('.delete-form');
            deleteForms.forEach(function (form) {
                form.addEventListener('submit', function (e) {
                    e.preventDefault();
                    if (confirm('Are you sure you want to delete this category? This cannot be undone.')) {
                        form.submit();
                    }
                });
            });
        });