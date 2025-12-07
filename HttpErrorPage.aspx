<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="HttpErrorPage.aspx.cs" Inherits="TrackerSQL.HttpErrorPage" %>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Oops! Something Went Wrong</title>
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <style>
        :root {
            --primary-brown: #302a20;
            --secondary-brown: #595939;
            --light-brown: #d0d0b9;
            --primary-green: #4e664d;
            --light-green: #e5f0d4;
            --secondary-green: #EDEBD2;
            --error-red: #d32f2f;
            --error-light: #ffebee;
        }

        * {
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }

        body {
            font-family: "Helvetica Neue", "Lucida Grande", "Segoe UI", Arial, sans-serif;
            background: linear-gradient(135deg, var(--light-brown) 0%, var(--light-green) 100%);
            min-height: 100vh;
            display: flex;
            align-items: center;
            justify-content: center;
            padding: 20px;
            color: var(--primary-brown);
        }

        .error-container {
            background: #fff;
            border-radius: 20px;
            box-shadow: 0 10px 30px rgba(0, 0, 0, 0.1);
            padding: 40px;
            max-width: 600px;
            width: 100%;
            text-align: center;
            border: 1px solid rgba(78, 102, 77, 0.2);
            animation: fadeInUp 0.6s ease-out;
        }

        @keyframes fadeInUp {
            from {
                opacity: 0;
                transform: translateY(30px);
            }
            to {
                opacity: 1;
                transform: translateY(0);
            }
        }

        .error-icon {
            width: 120px;
            height: 120px;
            margin: 0 auto 30px;
            background: var(--error-light);
            border-radius: 50%;
            display: flex;
            align-items: center;
            justify-content: center;
            font-size: 60px;
            color: var(--error-red);
            border: 3px solid var(--error-red);
            animation: pulse 2s infinite;
        }

        @keyframes pulse {
            0% {
                box-shadow: 0 0 0 0 rgba(211, 47, 47, 0.7);
            }
            70% {
                box-shadow: 0 0 0 10px rgba(211, 47, 47, 0);
            }
            100% {
                box-shadow: 0 0 0 0 rgba(211, 47, 47, 0);
            }
        }

        h1 {
            font-size: 2.5em;
            color: var(--primary-green);
            margin-bottom: 20px;
            font-weight: 300;
        }

        .error-subtitle {
            font-size: 1.2em;
            color: var(--secondary-brown);
            margin-bottom: 30px;
            line-height: 1.6;
        }

        .error-message {
            background: var(--error-light);
            border: 1px solid var(--error-red);
            border-radius: 10px;
            padding: 20px;
            margin: 30px 0;
            color: var(--error-red);
            font-weight: 500;
            font-size: 1em;
            line-height: 1.5;
            word-wrap: break-word;
            text-align: left;
        }

        .error-message:empty {
            display: none;
        }

        .action-buttons {
            display: flex;
            gap: 15px;
            justify-content: center;
            flex-wrap: wrap;
            margin-top: 30px;
        }

        .btn {
            padding: 12px 30px;
            border: none;
            border-radius: 25px;
            font-size: 1em;
            font-weight: 500;
            text-decoration: none;
            cursor: pointer;
            transition: all 0.3s ease;
            display: inline-flex;
            align-items: center;
            gap: 8px;
            min-width: 140px;
            justify-content: center;
        }

        .btn-primary {
            background: linear-gradient(135deg, var(--primary-green) 0%, var(--secondary-brown) 100%);
            color: white;
            box-shadow: 0 4px 15px rgba(78, 102, 77, 0.3);
        }

        .btn-primary:hover {
            transform: translateY(-2px);
            box-shadow: 0 6px 20px rgba(78, 102, 77, 0.4);
        }

        .btn-secondary {
            background: transparent;
            color: var(--primary-green);
            border: 2px solid var(--primary-green);
        }

        .btn-secondary:hover {
            background: var(--primary-green);
            color: white;
            transform: translateY(-2px);
        }

        .support-info {
            background: var(--secondary-green);
            border-radius: 15px;
            padding: 20px;
            margin-top: 30px;
            border-left: 4px solid var(--primary-green);
        }

        .support-info h3 {
            color: var(--primary-green);
            margin-bottom: 10px;
            font-size: 1.1em;
        }

        .support-info p {
            color: var(--secondary-brown);
            line-height: 1.5;
            margin: 5px 0;
        }

        /* Mobile responsiveness */
        @media (max-width: 768px) {
            body {
                padding: 15px;
            }

            .error-container {
                padding: 30px 20px;
            }

            h1 {
                font-size: 2em;
            }

            .error-icon {
                width: 100px;
                height: 100px;
                font-size: 50px;
            }

            .action-buttons {
                flex-direction: column;
                align-items: center;
            }

            .btn {
                width: 100%;
                max-width: 280px;
            }
        }

        @media (max-width: 480px) {
            .error-container {
                padding: 25px 15px;
            }

            h1 {
                font-size: 1.8em;
            }

            .error-subtitle {
                font-size: 1.1em;
            }

            .error-icon {
                width: 80px;
                height: 80px;
                font-size: 40px;
            }
        }
    </style>
</head>
<body>
    <form id="frmErrorPage" runat="server">
        <div class="error-container">
            <div class="error-icon">
                ⚠️
            </div>
            
            <h1>Oops! Something Went Wrong</h1>
            
            <div class="error-subtitle">
                We apologize for the inconvenience. An unexpected error has occurred while processing your request.
            </div>

            <div class="error-message">
                <asp:Label ID="lblErrorMessage" runat="server"></asp:Label>
            </div>

            <div class="action-buttons">
                <a href="Default.aspx" class="btn btn-primary">
                    🏠 Return Home
                </a>
                <a href="javascript:history.back()" class="btn btn-secondary">
                    ← Go Back
                </a>
            </div>

            <div class="support-info">
                <h3>Need Help?</h3>
                <p>If this problem persists, please contact our support team.</p>
                <p>Error occurred at: <strong><asp:Label ID="lblErrorTime" runat="server"></asp:Label></strong></p>
            </div>
        </div>
    </form>
</body>
</html>