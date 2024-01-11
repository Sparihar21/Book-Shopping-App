var dataTable;

$(document).ready(function () {
    loadDataTable();
})

function loadDataTable() {
    dataTable = $("#tbldata").dataTable({
        "ajax": {
            "url": "/Admin/Product/GetAll"
        }, "columns": [
            { "data": "title", "width": "15%" },
            { "data": "description", "width": "25%" },
            { "data": "author", "width": "15%" },
            { "data": "isbn", "width": "15%" },
            { "data": "price", "width": "15%" },
            {
                "data": "id", render: function (data) {
                    return `
                            <div class="text-center">
                                <a class="btn btn-info" href="/Admin/Product/Upsert/${data}"><i class="fas fa-edit"></i></a>
                                <a class="btn btn-danger" onclick=Delete("/Admin/Product/Delete/${data}")><i class="fas fa-trash-alt"></i></a>
</div>
`;
                }
            }]
    })
}

function Delete(url) {
    swal({
        title: "Do You Want To Delete This?",
        text: "Are You Sure?",
        icon: "warning",
        buttons: true,
        dangerMode: true
    }).then((willDelete) => {
        if (willDelete) {
            $.ajax({
                url: url,
                type: "DELETE",
                success: function (data) {
                    if (data.success) {
                        toastr.success(data.message);
                        $("#tbldata").DataTable().ajax.reload();
                    }
                    else {
                        toastr.error(data.message);
                    }
                }
            })
        }
    })
}
