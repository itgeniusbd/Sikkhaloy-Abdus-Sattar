var _rechargeSubmitting = false;

function confirmRecharge(btn) {
    if (_rechargeSubmitting) return false;

    var qtyInput = document.querySelector("input[id$='SMSQtyTextBox']");
    var qty = qtyInput ? (parseInt(qtyInput.value) || 0) : 0;
    if (qty <= 0) return false;

    _rechargeSubmitting = true;

    setTimeout(function () {
        btn.value = 'Processing...';
        btn.disabled = true;
    }, 0);

    setTimeout(function () {
        _rechargeSubmitting = false;
        btn.disabled = false;
        btn.value = 'Recharge & ShurjoPay';
    }, 30000);

    return true;
}

window.addEventListener('DOMContentLoaded', function () {
    var qtyInput = document.querySelector("input[id$='SMSQtyTextBox']");
    var lblCost  = document.querySelector("span[id$='TotalCostLabel']");
    var btn      = document.querySelector("input[id$='RechargeButton']");
    if (!qtyInput) return;

    qtyInput.addEventListener('input', function () {
        var qty   = parseInt(this.value) || 0;
        var total = (qty * 0.36).toFixed(2);
        if (lblCost) lblCost.textContent = qty > 0 ? 'Total: ' + total + ' Tk' : '';
        if (btn)     btn.disabled = false;
    });
});
