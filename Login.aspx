<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="SocietyManagement.Login" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>Society Management - Login</title>
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet" />
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.4.0/css/all.min.css">
    <link rel="preconnect" href="https://fonts.googleapis.com">
    <link href="https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700&display=swap" rel="stylesheet">
    
    <style>
        * {
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }

        body {
            font-family: 'Inter', sans-serif;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            min-height: 100vh;
            display: flex;
            align-items: center;
            justify-content: center;
            padding: 20px;
        }

        .login-container {
            background: white;
            border-radius: 20px;
            box-shadow: 0 20px 60px rgba(0, 0, 0, 0.3);
            overflow: hidden;
            max-width: 1000px;
            width: 100%;
            display: flex;
        }

        .login-left {
            flex: 1;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            padding: 60px 40px;
            color: white;
            display: flex;
            flex-direction: column;
            justify-content: center;
        }

        .login-left h1 {
            font-size: 36px;
            font-weight: 700;
            margin-bottom: 20px;
        }

        .login-left p {
            font-size: 16px;
            opacity: 0.9;
            line-height: 1.6;
        }

        .feature-list {
            margin-top: 40px;
            list-style: none;
        }

        .feature-list li {
            margin-bottom: 15px;
            display: flex;
            align-items: center;
            gap: 12px;
        }

        .feature-list i {
            width: 24px;
            height: 24px;
            background: rgba(255, 255, 255, 0.2);
            border-radius: 50%;
            display: flex;
            align-items: center;
            justify-content: center;
            font-size: 12px;
        }

        .login-right {
            flex: 1;
            padding: 60px 40px;
        }

        .login-header {
            text-align: center;
            margin-bottom: 40px;
        }

        .logo-box {
            width: 60px;
            height: 60px;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            border-radius: 12px;
            display: flex;
            align-items: center;
            justify-content: center;
            margin: 0 auto 20px;
        }

        .logo-box i {
            font-size: 28px;
            color: white;
        }

        .login-header h2 {
            font-size: 28px;
            font-weight: 700;
            color: #1e293b;
            margin-bottom: 8px;
        }

        .login-header p {
            color: #64748b;
            font-size: 14px;
        }

        .form-group {
            margin-bottom: 24px;
        }

        .form-label {
            display: block;
            font-weight: 600;
            color: #1e293b;
            margin-bottom: 8px;
            font-size: 14px;
        }

        .input-group {
            position: relative;
        }

        .input-icon {
            position: absolute;
            left: 16px;
            top: 50%;
            transform: translateY(-50%);
            color: #94a3b8;
            font-size: 16px;
        }

        .form-control {
            width: 100%;
            padding: 14px 16px 14px 48px;
            border: 2px solid #e2e8f0;
            border-radius: 10px;
            font-size: 15px;
            transition: all 0.3s;
            background: #f8fafc;
        }

        .form-control:focus {
            outline: none;
            border-color: #667eea;
            background: white;
            box-shadow: 0 0 0 4px rgba(102, 126, 234, 0.1);
        }

        .btn-login {
            width: 100%;
            padding: 14px;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            border: none;
            border-radius: 10px;
            color: white;
            font-size: 16px;
            font-weight: 600;
            cursor: pointer;
            transition: all 0.3s;
            margin-top: 10px;
        }

        .btn-login:hover {
            transform: translateY(-2px);
            box-shadow: 0 10px 20px rgba(102, 126, 234, 0.3);
        }

        .error-message {
            background: #fee;
            border-left: 4px solid #ef4444;
            padding: 12px;
            border-radius: 8px;
            color: #dc2626;
            font-size: 14px;
            margin-top: 16px;
        }

        .forgot-password {
            text-align: right;
            margin-top: 12px;
        }

        .forgot-password a {
            color: #667eea;
            text-decoration: none;
            font-size: 14px;
            font-weight: 500;
        }

        .forgot-password a:hover {
            text-decoration: underline;
        }

        @media (max-width: 768px) {
            .login-container {
                flex-direction: column;
            }

            .login-left {
                padding: 40px 30px;
            }

            .login-right {
                padding: 40px 30px;
            }

            .feature-list {
                display: none;
            }
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="login-container">
            <div class="login-left">
                <h1>Society Management System</h1>
                <p>Streamline your society operations with our comprehensive management platform.</p>
                
                <ul class="feature-list">
                    <li>
                        <i class="fas fa-check"></i>
                        <span>Manage members and units efficiently</span>
                    </li>
                    <li>
                        <i class="fas fa-check"></i>
                        <span>Track finances and maintenance bills</span>
                    </li>
                    <li>
                        <i class="fas fa-check"></i>
                        <span>Handle complaints and announcements</span>
                    </li>
                    <li>
                        <i class="fas fa-check"></i>
                        <span>Monitor visitors and security</span>
                    </li>
                </ul>
            </div>

            <div class="login-right">
                <div class="login-header">
                    <div class="logo-box">
                        <i class="fas fa-building"></i>
                    </div>
                    <h2>Welcome Back</h2>
                    <p>Please login to your account</p>
                </div>

                <div class="form-group">
                    <label class="form-label">Username</label>
                    <div class="input-group">
                        <i class="fas fa-user input-icon"></i>
                        <asp:TextBox ID="txtUsername" runat="server" CssClass="form-control" 
                            placeholder="Enter your username"></asp:TextBox>
                    </div>
                </div>

                <div class="form-group">
                    <label class="form-label">Password</label>
                    <div class="input-group">
                        <i class="fas fa-lock input-icon"></i>
                        <asp:TextBox ID="txtPassword" runat="server" TextMode="Password" 
                            CssClass="form-control" placeholder="Enter your password"></asp:TextBox>
                    </div>
                </div>

                <div class="forgot-password">
                    <a href="#">Forgot Password?</a>
                </div>

                <asp:Button ID="btnLogin" runat="server" Text="Sign In" 
                    CssClass="btn-login" OnClick="btnLogin_Click" />

                <asp:Label ID="lblMessage" runat="server" CssClass="error-message" 
                    Visible="false"></asp:Label>
            </div>
        </div>
    </form>
</body>
</html>