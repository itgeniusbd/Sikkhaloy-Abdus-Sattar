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
        var qty          = parseInt(this.value) || 0;
        var invoice      = qty * 0.36;
        var gwCharge     = Math.round(invoice / 1000 * 19 * 100) / 100;
        var totalPayable = invoice + gwCharge;
        if (lblCost) {
            lblCost.textContent = qty > 0
                ? 'বিল: ' + invoice.toFixed(2) + ' + চার্জ: ' + gwCharge.toFixed(2) + ' = মোট: ' + totalPayable.toFixed(2) + ' Tk'
                : '';
        }
        if (btn) btn.disabled = false;
    });
});
