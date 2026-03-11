const AdminTools = {
    // 1. Khởi tạo TinyMCE tối ưu 
    initRichEditor: function (selector, uploadUrl) {
        tinymce.init({
            selector: selector,
            plugins: 'image code table lists link wordcount',
            toolbar: 'undo redo | formatselect | bold italic | alignleft aligncenter alignright | bullist numlist | table | image | code', // 
            images_upload_url: uploadUrl, // 
            automatic_uploads: true,
            images_upload_base_path: '/uploads/products',
            height: 400,
            setup: function (editor) {
                editor.on('change', function () {
                    tinymce.triggerSave(); // Đảm bảo HTML được cập nhật vào textarea để lưu xuống DB 
                });
            }
        });
    },

    // 2. Logic tạo Slug chuẩn SEO 
    generateSlug: function (text) {
        return text.toString().toLowerCase()
            .replace(/á|à|ả|ạ|ã|ă|ắ|ằ|ẳ|ẵ|ặ|â|ấ|ầ|ẩ|ẫ|ậ/g, 'a')
            .replace(/é|è|ẻ|ẽ|ẹ|ê|ế|ề|ể|ễ|ệ/g, 'e')
            .replace(/i|í|ì|ỉ|ĩ|ị/g, 'i')
            .replace(/ó|ò|ỏ|õ|ọ|ô|ố|ồ|ổ|ỗ|ộ|ơ|ớ|ờ|ở|ỡ|ợ/g, 'o')
            .replace(/ú|ù|ủ|ũ|ụ|ư|ứ|ừ|ử|ữ|ự/g, 'u')
            .replace(/ý|ỳ|ỷ|ỹ|ỵ/g, 'y')
            .replace(/đ/g, 'd')
            .replace(/\s+/g, '-')           
            .replace(/[^\w\-]+/g, '')       
            .replace(/\-\-+/g, '-')
            .replace(/^-+/, '')
            .replace(/-+$/, '');
    },

    bindAutoSlug: function (sourceId, targetId) {
        document.getElementById(sourceId).addEventListener('input', function () {
            document.getElementById(targetId).value = AdminTools.generateSlug(this.value);
        });
    }
};