// KLDShop - Main JavaScript

// Format Currency
function formatCurrency(value) {
    return value.toString().replace(/\B(?=(\d{3})+(?!\d))/g, '.') + 'đ';
}

// Format Number
function formatNumber(value) {
    return value.toString().replace(/\B(?=(\d{3})+(?!\d))/g, '.');
}

// Parse Currency String
function parseCurrency(currencyString) {
    return parseInt(currencyString.replace(/[^\d]/g, ''));
}

// Cart Badge Update
function updateCartBadge() {
    $.get('/Cart/GetCartSummary', function (data) {
        if (data && data.itemCount >= 0) {
            $('#cart-count').text(data.itemCount);
        }
    }).fail(function () {
        console.log('Unable to update cart badge');
    });
}

// Add to Cart
function addToCart(productId, quantity = 1) {
    if (!productId || productId <= 0) {
        alert('Sản phẩm không hợp lệ');
        return;
    }

    if (!quantity || quantity <= 0) {
        alert('Số lượng phải lớn hơn 0');
        return;
    }

    $.ajax({
        type: 'POST',
        url: '/Cart/AddToCart',
        data: {
            productId: productId,
            quantity: quantity
        },
        success: function (response) {
            if (response.success) {
                showNotification('success', response.message || 'Thêm vào giỏ hàng thành công');
                updateCartBadge();
            } else {
                showNotification('danger', response.message || 'Lỗi khi thêm vào giỏ hàng');
            }
        },
        error: function () {
            showNotification('danger', 'Lỗi kết nối');
        }
    });
}

// Remove from Cart
function removeFromCart(cartItemId) {
    if (!confirm('Bạn có chắc muốn xóa sản phẩm này?')) {
        return;
    }

    $.ajax({
        type: 'POST',
        url: '/Cart/RemoveFromCart',
        data: { cartItemId: cartItemId },
        success: function (response) {
            if (response.success) {
                location.reload();
            }
        }
    });
}

// Update Cart Item
function updateCartItem(cartItemId, quantity) {
    if (quantity <= 0) {
        removeFromCart(cartItemId);
        return;
    }

    $.ajax({
        type: 'POST',
        url: '/Cart/UpdateCartItem',
        data: {
            cartItemId: cartItemId,
            quantity: quantity
        },
        success: function (response) {
            if (response.success) {
                location.reload();
            }
        }
    });
}

// Clear Cart
function clearCart() {
    if (!confirm('Bạn có chắc muốn xóa tất cả sản phẩm?')) {
        return;
    }

    $.ajax({
        type: 'POST',
        url: '/Cart/ClearCart',
        success: function (response) {
            if (response.success) {
                location.reload();
            }
        }
    });
}

// Show Notification
function showNotification(type, message) {
    const alertClass = {
        'success': 'alert-success',
        'danger': 'alert-danger',
        'warning': 'alert-warning',
        'info': 'alert-info'
    };

    const alertDiv = `
        <div class="alert ${alertClass[type] || 'alert-info'} alert-dismissible fade show" role="alert" style="position: fixed; top: 80px; right: 20px; z-index: 1000; max-width: 400px;">
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
        </div>
    `;

    $('body').append(alertDiv);

    // Auto dismiss after 5 seconds
    setTimeout(() => {
        $('.alert').fadeOut(function () {
            $(this).remove();
        });
    }, 5000);
}

// Initialize on Document Ready
$(document).ready(function () {
    // Update cart badge on page load
    updateCartBadge();

    // Format all prices
    formatPrices();

    // Initialize tooltips if Bootstrap tooltips are used
    if (typeof bootstrap !== 'undefined') {
        const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
        tooltipTriggerList.map(function (tooltipTriggerEl) {
            return new bootstrap.Tooltip(tooltipTriggerEl);
        });
    }

    // Smooth scroll for anchor links
    $('a[href^="#"]').on('click', function (e) {
        e.preventDefault();
        const target = $(this.getAttribute('href'));
        if (target.length) {
            $('html, body').stop().animate({
                scrollTop: target.offset().top - 100
            }, 1000);
        }
    });
});

// Format all prices on page
function formatPrices() {
    $('.product-price, .price-display').each(function () {
        const value = parseInt($(this).text().replace(/[^\d]/g, ''));
        if (!isNaN(value)) {
            $(this).text(formatCurrency(value));
        }
    });
}

// Debounce function for search
function debounce(func, wait) {
    let timeout;
    return function executedFunction(...args) {
        const later = () => {
            clearTimeout(timeout);
            func(...args);
        };
        clearTimeout(timeout);
        timeout = setTimeout(later, wait);
    };
}

// Search products
const searchProducts = debounce(function (query) {
    if (query.length < 2) {
        return;
    }

    $.ajax({
        type: 'GET',
        url: '/api/products',
        data: { search: query },
        success: function (response) {
            if (response.success) {
                displaySearchResults(response.data);
            }
        }
    });
}, 500);

// Display search results
function displaySearchResults(results) {
    const resultsContainer = $('#search-results');
    if (!resultsContainer.length) {
        return;
    }

    resultsContainer.empty();
    if (results.length === 0) {
        resultsContainer.html('<div class="alert alert-info">Không tìm thấy sản phẩm</div>');
        return;
    }

    results.forEach(product => {
        const html = `
            <div class="search-result-item p-3 border-bottom">
                <a href="/Product/Details/${product.id}">
                    <strong>${product.name}</strong>
                </a>
                <br>
                <small class="text-muted">${formatCurrency(product.price)}</small>
            </div>
        `;
        resultsContainer.append(html);
    });
}

// Export functions for global use
window.KLDShop = {
    formatCurrency,
    formatNumber,
    parseCurrency,
    updateCartBadge,
    addToCart,
    removeFromCart,
    updateCartItem,
    clearCart,
    showNotification,
    searchProducts,
    formatPrices
};
