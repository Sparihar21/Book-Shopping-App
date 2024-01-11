var dataTables;

$(document).ready(function () {
    loadDataTable();
});

function loadDataTable() {
  dataTables=  $("#tbldata").dataTable({
        "ajax": {
            "url": "/Admin/Category/GetAll"
        },
        "columns": [{ "data": "name", "width": "70%" },
        {
            "data": "id",
            "render": function (data) {
                return `<div class="text-center">
                            <a href="/Admin/Category/Upsert/${data}" class="btn btn-info"><i class="fas fa-edit"></i></a>
                            <a onclick="Delete('/Admin/Category/Delete/${data}')" class="btn btn-danger"><i class="fas fa-trash-alt"></i></a>

`;
            }
        }
        ]
    })

};

function Delete(url) {
    swal({
        title: "Want To Delete This",
        text: "Delete Everything",
        icon: "warning",
        buttons: true,
        dangerMode: true // Corrected spelling
    }).then((willDelete) => {
        if (willDelete) {
            $.ajax({
                url: url,
                type: "DELETE",
                success: function (data) {
                    if (data.success) {
                        toastr.success(data.message);
                        $('#tbldata').DataTable().ajax.reload();
                        //dataTables.ajax.reload();
                      
                        
                    } else {
                        toastr.error(data.message);
                    }
                },
                error: function (xhr, status, error) {
                    toastr.error("Error occurred while deleting.");
                    console.error(xhr.responseText);
                }
            });
        }
    });
}

