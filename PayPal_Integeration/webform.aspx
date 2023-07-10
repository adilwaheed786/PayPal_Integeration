<%@ Page Async="true" Language="C#" AutoEventWireup="true" CodeBehind="webform.aspx.cs" Inherits="PayPal_Integeration.webform" %>

<!DOCTYPE html>
<html>
<head>
    <title>Donation Page</title>
    <style>
        .gdlr-paypal-form-head {
            font-size: 24px;
            font-weight: bold;
            margin-bottom: 20px;
        }

        .gdlr-paypal-amount-wrapper {
            margin-bottom: 20px;
        }

        .gdlr-head {
            font-size: 18px;
            font-weight: bold;
            margin-bottom: 10px;
        }

        .gdlr-amount-button {
            display: inline-block;
            padding: 5px 10px;
            background-color: #ebebeb;
            color: #333;
            text-decoration: none;
            margin-right: 10px;
            border-radius: 5px;
            cursor: pointer;
        }

            .gdlr-amount-button.active {
                background-color: #333;
                color: #fff;
            }

        .custom-amount {
            width: 50%;
            padding: 5px;
            margin-top: 10px;
            border: 1px solid #ccc;
            border-radius: 5px;
        }

        .gdlr-recurring-payment-wrapper {
            margin-top: 20px;
        }

        .gdlr-subhead {
            font-size: 16px;
        }

        .gdlr-recurring-option {
            margin-left: 10px;
        }

        .gdlr-recurring-time-wrapper {
            margin-top: 10px;
        }

        input[type="submit"] {
            background-color: #333;
            color: #fff;
            padding: 10px 20px;
            border: none;
            border-radius: 5px;
            font-size: 16px;
            cursor: pointer;
        }

            input[type="submit"]:hover {
                background-color: #555;
            }
    </style>
    <style>
        #paypal-button-container {
            position: fixed;
            top: 50%;
            left: 50%;
            transform: translate(-50%, -50%);
            background-color: #fff;
            border: 1px solid #ccc;
            padding: 20px;
            box-shadow: 0 2px 6px rgba(0, 0, 0, 0.3);
            z-index: 9999;
            display: none; /* Hide the container by default */
        }

        .backdrop {
            position: fixed;
            top: 0;
            left: 0;
            width: 100%;
            height: 100%;
            background-color: rgba(0, 0, 0, 0.5);
            z-index: 9998;
            display: none; /* Hide the backdrop by default */
        }
    </style>
  <script src="https://www.paypal.com/sdk/js?client-id=YOUR_CLIENT_ID&vault=true&intent=subscription">
  </script> 
</head>
<body>
    <form id="form1" runat="server">
        <div class="gdlr-paypal-amount-wrapper">
            <span class="gdlr-head">How much would you like to donate?</span>
            <input type="text" class="custom-amount" placeholder="Enter Amount In $" id="userAmount" runat="server" onkeypress="return isNumberKey(event)">
            <label id="errorMessage" style="color: red; display: none;"></label>
            <label id="amountErrorMessage" style="color: red; display: none;">Please enter a  Amount.</label>
            <div class="clear"></div>

            <!-- recurring-1 -->
            <div class="gdlr-recurring-payment-wrapper">
                <span class="gdlr-head">Would you like to make regular donations?</span>
                <span class="gdlr-subhead">I would like to make </span>
                <asp:DropDownList ID="ddlRecurringPeriod" runat="server" CssClass="gdlr-recurring-option" onchange="showHideRecurringTimes()">
                    <asp:ListItem Value="0">a one time</asp:ListItem>
                    <asp:ListItem Value="W">weekly</asp:ListItem>
                    <asp:ListItem Value="M">monthly</asp:ListItem>
                    <asp:ListItem Value="Y">yearly</asp:ListItem>
                </asp:DropDownList>
                <span class="gdlr-subhead">donation(s)</span>
                <div class="gdlr-recurring-time-wrapper" id="recurringTimesWrapper" style="display: none">
                    <span class="gdlr-subhead">How many times would you like this to recur? (including this payment)*</span>
                    <asp:DropDownList ID="ddlRecurringTimes" runat="server" CssClass="gdlr-recurring-option">
                        <asp:ListItem Value="2">2</asp:ListItem>
                        <asp:ListItem Value="3">3</asp:ListItem>
                        <asp:ListItem Value="4">4</asp:ListItem>
                        <asp:ListItem Value="5">5</asp:ListItem>
                        <asp:ListItem Value="6">6</asp:ListItem>
                        <asp:ListItem Value="7">7</asp:ListItem>
                        <asp:ListItem Value="8">8</asp:ListItem>
                        <asp:ListItem Value="9">9</asp:ListItem>
                        <asp:ListItem Value="10">10</asp:ListItem>
                        <asp:ListItem Value="11">11</asp:ListItem>
                        <asp:ListItem Value="12">12</asp:ListItem>
                    </asp:DropDownList>
                </div>

            </div>
            <!-- recurring-2 -->
        </div>
        <div>
            <asp:Button ID="btnDonate" runat="server" Text="Donate" OnClick="btnDonate_Click1" OnClientClick="return validateAmount()" />
        </div>
    </form>
    <div id="paypal-button-container"></div>
    <div class="backdrop"></div>
</body>
<script type="text/javascript">
    function isNumberKey(evt) {
        var charCode = (evt.which) ? evt.which : event.keyCode;
        if (charCode > 31 && (charCode < 48 || charCode > 57)) {
            document.getElementById("errorMessage").innerHTML = "Please enter a valid number";
            document.getElementById("errorMessage").style.display = "block";
            return false;
        }
        document.getElementById("errorMessage").style.display = "none";
        return true;
    }
    function showHideRecurringTimes() {
        var recurringPeriod = document.getElementById('<%= ddlRecurringPeriod.ClientID %>').value;
        var recurringTimesWrapper = document.getElementById('recurringTimesWrapper');

        if (recurringPeriod === '0') {
            recurringTimesWrapper.style.display = 'none';
        } else {
            recurringTimesWrapper.style.display = 'block';
        }
    }
    function validateAmount() {
        var amount = document.getElementById('userAmount').value;
        var amountErrorMessage = document.getElementById('amountErrorMessage');

        if (amount === null || amount.trim() === '') {
            amountErrorMessage.style.display = 'block';
            return false;
        }

        amountErrorMessage.style.display = 'none';
        return true;
    }
    document.getElementById('userAmount').addEventListener('input', function () {
        var amountErrorMessage = document.getElementById('amountErrorMessage');
        amountErrorMessage.style.display = 'none';
    });
</script>
<script>
    var subscriptionPlanId = '<%= Session["SubscriptionPlanId"] %>';
    if (subscriptionPlanId === '') {
        // Hide the button if the plan ID is empty
        document.querySelector('#paypal-button-container').style.display = 'none';
        document.querySelector('.backdrop').style.display = 'none'; // Hide the backdrop
    } else {
        document.querySelector('#paypal-button-container').style.display = 'block';
        document.querySelector('.backdrop').style.display = 'block';
        paypal.Buttons({
            createSubscription: function (data, actions) {
                return actions.subscription.create({
                    'plan_id': subscriptionPlanId // Creates the subscription
                });
            },
            onApprove: function (data, actions) {
                // Make a server-side call to clear the session value
                document.querySelector('#paypal-button-container').style.display = 'none';
                document.querySelector('.backdrop').style.display = 'none'; // Hide the backdrop
                //alert('You have successfully subscribed to ' + data.subscriptionID); // Optional message given to subscriber

            }
        }).render('#paypal-button-container'); // Renders the PayPal button
    }
</script>

</html>
