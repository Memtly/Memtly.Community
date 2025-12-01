const path = require('path');
const MiniCssExtractPlugin = require('mini-css-extract-plugin');
const webpack = require('webpack');
const glob = require('glob');

const themeEntries = glob.sync('./wwwroot/components/themes/*.css').reduce((acc, filePath) => {
    const normalizedPath = filePath.startsWith('./') ? filePath : `./${filePath}`;
    const themeName = path.basename(filePath, '.css');
    acc[`themes/${themeName}`] = normalizedPath;
    return acc;
}, {});

module.exports = {
    entry: {
        main: './wwwroot/js/index.js',
        gallery_selector: './wwwroot/js/pages/gallery/selector.js',
        login: './wwwroot/js/pages/account/login.js',
        registration: './wwwroot/js/pages/account/registration.js',
        forgot_password: './wwwroot/js/pages/account/forgot-password.js',
        password_reset: './wwwroot/js/pages/account/password-reset.js',
        ...themeEntries
    },
    output: {
        path: path.resolve(__dirname, 'wwwroot/dist'),
        filename: '[name].bundle.js',
        publicPath: '/dist/',
        clean: true
    },
    module: {
        rules: [
            {
                test: /\.js$/,
                exclude: /node_modules/,
                use: {
                    loader: 'babel-loader',
                    options: {
                        presets: ['@babel/preset-env']
                    }
                }
            },
            {
                test: /\.css$/,
                use: [
                    MiniCssExtractPlugin.loader,
                    'css-loader'
                ]
            },
            {
                test: /\.(woff|woff2|eot|ttf|otf)$/,
                type: 'asset/resource',
                generator: {
                    filename: 'fonts/[name][ext]'
                }
            },
            {
                test: /\.(svg|png|jpg|jpeg|gif)$/,
                type: 'asset/resource',
                generator: {
                    filename: 'images/[name][ext]'
                }
            }
        ]
    },
    plugins: [
        new MiniCssExtractPlugin({
            filename: '[name].css'
        }),
        new webpack.ProvidePlugin({
            $: 'jquery',
            jQuery: 'jquery',
            'window.jQuery': 'jquery',
            Popper: ['@popperjs/core', 'default']
        })
    ],
    mode: 'development',
    watch: false
};